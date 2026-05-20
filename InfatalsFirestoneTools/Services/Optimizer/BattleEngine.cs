using InfatalsFirestoneTools.Models;
using System.Runtime.CompilerServices;

namespace InfatalsFirestoneTools.Services.Optimizer;

public sealed class BattleEngine
{
    // ── Game constants (private) ──────────────────────────────────────────────

    private const int MaxRounds = 20;
    private static ReadOnlySpan<int> AttackOrder => [0, 1, 2, 4, 3];

    // ── Public API ────────────────────────────────────────────────────────────
    public bool RunBatch(
        ReadOnlySpan<BattleMember> initialPlayers,
        ReadOnlySpan<BattleMember> initialEnemies,
        int runs,
        ref XorShiftState rng)
    {
        // Allocate the stack buffers once for this execution frame
        Span<BattleMember> players = stackalloc BattleMember[initialPlayers.Length];
        Span<BattleMember> enemies = stackalloc BattleMember[initialEnemies.Length];

        for (int i = 0; i < runs; i++)
        {
            // Reset the stage instantly via SIMD block copies before each battle
            initialPlayers.CopyTo(players);
            initialEnemies.CopyTo(enemies);

            // Execute the core simulation
            if (RunSimulation(players, enemies, ref rng).PlayerWon)
            {
                // Optimization: If we only care if a victory is possible to advance,
                // we can return true early the second we see a single win!
                return true;
            }
        }

        return false;
    }

    private BattleResult RunSimulation(Span<BattleMember> players, Span<BattleMember> enemies, ref XorShiftState rng)
    {
        int alivePlayers = players.Length;
        int aliveEnemies = enemies.Length;
        int round = 0;

        Span<int> targetBuffer = stackalloc int[5];

        while (round < MaxRounds && alivePlayers > 0 && aliveEnemies > 0)
        {
            AttackPhase(players, enemies, ref aliveEnemies, true, ref rng, targetBuffer);
            if (aliveEnemies <= 0) break;

            AttackPhase(enemies, players, ref alivePlayers, false, ref rng, targetBuffer);
            round++;
        }

        return new BattleResult(aliveEnemies <= 0 && alivePlayers > 0, round);
    }


    public BattleResult Run(ReadOnlySpan<BattleMember> initialPlayers, ReadOnlySpan<BattleMember> initialEnemies, ref XorShiftState rng)
    {
        Span<BattleMember> players = stackalloc BattleMember[initialPlayers.Length];
        Span<BattleMember> enemies = stackalloc BattleMember[initialEnemies.Length];

        initialPlayers.CopyTo(players);
        initialEnemies.CopyTo(enemies);

        int alivePlayers = players.Length;
        int aliveEnemies = enemies.Length;
        int round = 0;

        Span<int> targetBuffer = stackalloc int[5];

        while (round < MaxRounds && alivePlayers > 0 && aliveEnemies > 0)
        {
            AttackPhase(players, enemies, ref aliveEnemies, true, ref rng, targetBuffer);
            if (aliveEnemies <= 0) break;

            AttackPhase(enemies, players, ref alivePlayers, false, ref rng, targetBuffer);
            round++;
        }

        return new BattleResult(aliveEnemies <= 0 && alivePlayers > 0, round);
    }

    // ── Attack phase ──────────────────────────────────────────────────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AttackPhase(Span<BattleMember> attackers, Span<BattleMember> defenders, ref int aliveDefenders, bool enableAbilities, ref XorShiftState rng, Span<int> targetBuffer)
    {
        for (int i = 0; i < AttackOrder.Length; i++)
        {
            int slot = AttackOrder[i];
            if (slot >= attackers.Length) continue;

            ref BattleMember attacker = ref attackers[slot];
            if (attacker.IsDead) continue;

            int targetIdx = FirstAliveTarget(defenders);
            if (targetIdx < 0) break;

            ref BattleMember defender = ref defenders[targetIdx];

            if (Damage(ref defender, DamageTaken(attacker.Dmg, defender.Arm)))
                aliveDefenders--;

            if (enableAbilities && attacker.HasAbility)
            {
                if (rng.NextDouble() < attacker.OverdriveChance)
                    ExecuteAbility(slot, ref attacker, attackers, defenders, ref aliveDefenders, ref rng, targetBuffer);
            }
        }
    }

    // ── Ability execution ────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ExecuteAbility(int casterIndex, ref BattleMember caster, Span<BattleMember> playerTeam, Span<BattleMember> enemyTeam, ref int aliveEnemies, ref XorShiftState rng, Span<int> targetBuffer)
    {
        Span<BattleMember> targets = caster.TargetType == AbilityTargetType.Enemy ? enemyTeam : playerTeam;

        int actualCount = SelectTargets(targets, ref caster, casterIndex, targetBuffer, ref rng);
        if (actualCount == 0) return;

        double baseStat = (caster.ScaleStat == AbilityScaleStat.Health) ? caster.MaxHp : caster.Dmg;
        double finalValue = baseStat + caster.MultiplierLog;

        if (caster.Effect == AbilityEffect.Heal)
        {
            for (int i = 0; i < actualCount; i++)
                Heal(ref targets[targetBuffer[i]], finalValue);
        }
        else
        {
            for (int i = 0; i < actualCount; i++)
            {
                ref BattleMember t = ref targets[targetBuffer[i]];
                if (Damage(ref t, DamageTaken(finalValue, t.Arm)))
                    aliveEnemies--;
            }
        }
    }

    private static int SelectTargets(Span<BattleMember> team, ref BattleMember caster, int casterIndex, Span<int> output, ref XorShiftState rng)
    {
        Span<int> aliveIndices = stackalloc int[team.Length];
        int aliveCount = 0;
        for (int i = 0; i < team.Length; i++)
        {
            if (!team[i].IsDead)
                aliveIndices[aliveCount++] = i;
        }

        if (aliveCount == 0) return 0;

        int numToTake = Math.Min(caster.NumTargets, aliveCount);

        switch (caster.TargetPosition)
        {
            case AbilityTargetPosition.Self:
                output[0] = casterIndex;
                return 1;
            case AbilityTargetPosition.All:
                aliveIndices[..aliveCount].CopyTo(output);
                return aliveCount;
            case AbilityTargetPosition.Lowest:
                int lowest = aliveIndices[0];
                for (int i = 1; i < aliveCount; i++)
                {
                    if (team[aliveIndices[i]].Hp < team[lowest].Hp)
                        lowest = aliveIndices[i];
                }
                output[0] = lowest;
                return 1;
            case AbilityTargetPosition.Last:
                for (int i = 0; i < numToTake; i++)
                    output[i] = aliveIndices[aliveCount - 1 - i];
                return numToTake;
            case AbilityTargetPosition.Random:
                // Fisher-Yates using custom RNG
                for (int i = 0; i < numToTake; i++)
                {
                    int j = rng.Next(i, aliveCount);
                    (aliveIndices[i], aliveIndices[j]) = (aliveIndices[j], aliveIndices[i]);
                    output[i] = aliveIndices[i];
                }
                return numToTake;
            default:
                return 0;
        }
    }

    // ── Mutation helpers (ref avoids struct copies) ───────────────────────────
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Damage(ref BattleMember target, double logAmount)
    {
        if (target.IsDead || double.IsNegativeInfinity(logAmount)) return false;

        target.Hp = LogSubtract(target.Hp, logAmount);
        if (double.IsNegativeInfinity(target.Hp))
        {
            target.IsDead = true;
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Heal(ref BattleMember target, double logAmount)
    {
        if (target.IsDead) return;
        target.Hp = LogAdd(target.Hp, logAmount);
        if (target.Hp > target.MaxHp) target.Hp = target.MaxHp;
    }

    // ── Query helpers ─────────────────────────────────────────────────────────

    private static int FirstAliveTarget(Span<BattleMember> team)
    {
        foreach (int slot in AttackOrder)
            if (slot < team.Length && !team[slot].IsDead) return slot;
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double DamageTaken(double logDmg, double logArmor)
    {
        return logDmg > logArmor ? LogSubtract(logDmg, logArmor) : double.NegativeInfinity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double LogAdd(double logA, double logB)
    {
        if (double.IsNegativeInfinity(logA)) return logB;
        if (double.IsNegativeInfinity(logB)) return logA;

        double max = logA > logB ? logA : logB;
        double diff = (logA > logB ? logB : logA) - max;

        if (diff < -16.0) return max;

        // .NET 10 Specific: Uses hardware-accelerated 10^x and log10(1+x)
        return max + double.Log10P1(double.Exp10(diff));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double LogSubtract(double logA, double logB)
    {
        if (logA <= logB) return double.NegativeInfinity;
        double diff = logB - logA;

        if (diff < -16.0) return logA;

        // log10(10^a - 10^b) = a + log10(1 - 10^(b-a))
        return logA + double.Log10P1(-double.Exp10(diff));
    }
}

// ── Supporting types ──────────────────────────────────────────────────────────

public sealed record BattleResult(bool PlayerWon, int Rounds);