using BreakEternity;
using InfatalsFirestoneTools.Models;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace InfatalsFirestoneTools.Services.Optimizer;

public sealed class BattleEngine
{
    // ── Game constants (private) ──────────────────────────────────────────────

    private const int MaxRounds = 20;
    private static ReadOnlySpan<int> AttackOrder => [0, 1, 2, 4, 3];

    // ── Public API ────────────────────────────────────────────────────────────
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ToLog(BigDouble value)
    {
        return value > 0 ? value.log10().toDouble() : double.NegativeInfinity;
    }

    public BattleResult Run(IReadOnlyList<ComputedMachine> playerTeam, IReadOnlyList<MachineStats> enemyStats, bool enableAbilities = true)
    {
        BattleMember[] playerArr = ArrayPool<BattleMember>.Shared.Rent(playerTeam.Count);
        BattleMember[] enemyArr = ArrayPool<BattleMember>.Shared.Rent(enemyStats.Count);

        try
        {
            Span<BattleMember> players = playerArr.AsSpan(0, playerTeam.Count);
            Span<BattleMember> enemies = enemyArr.AsSpan(0, enemyStats.Count);

            // Initialize members
            for (int i = 0; i < players.Length; i++)
            {
                MachineStats stats = playerTeam[i].BattleStats;
                players[i] = new BattleMember(playerTeam[i], ToLog(stats.Health), ToLog(stats.Damage), ToLog(stats.Armor));
            }

            for (int i = 0; i < enemies.Length; i++)
            {
                MachineStats stats = enemyStats[i];
                enemies[i] = new BattleMember(null, ToLog(stats.Health), ToLog(stats.Damage), ToLog(stats.Armor));
            }

            int alivePlayers = players.Length;
            int aliveEnemies = enemies.Length;
            int round = 0;

            while (round < MaxRounds && alivePlayers > 0 && aliveEnemies > 0)
            {
                AttackPhase(players, enemies, ref aliveEnemies, enableAbilities);
                if (aliveEnemies <= 0) break;

                AttackPhase(enemies, players, ref alivePlayers, false);
                round++;
            }

            return new BattleResult(aliveEnemies <= 0 && alivePlayers > 0, round);
        }
        finally
        {
            ArrayPool<BattleMember>.Shared.Return(playerArr);
            ArrayPool<BattleMember>.Shared.Return(enemyArr);
        }
    }

    // ── Attack phase ──────────────────────────────────────────────────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AttackPhase(Span<BattleMember> attackers, Span<BattleMember> defenders, ref int aliveDefenders, bool enableAbilities)
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

            if (enableAbilities && attacker.Source?.Ability is { } ability)
            {
                if (Random.Shared.NextDouble() < Calculator.CalculateOverdrive(attacker.Source))
                    ExecuteAbility(slot, ref attacker, attackers, defenders, ability, ref aliveDefenders);
            }
        }
    }

    // ── Ability execution ────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ExecuteAbility(int casterIndex, ref BattleMember caster, Span<BattleMember> playerTeam, Span<BattleMember> enemyTeam, Ability ability, ref int aliveEnemies)
    {
        Span<BattleMember> targets = ability.TargetType == AbilityTargetType.Enemy ? enemyTeam : playerTeam;

        Span<int> selectedIndices = stackalloc int[5];
        int actualCount = SelectTargets(targets, ability, casterIndex, selectedIndices);

        if (actualCount == 0) return;

        // Calculate ability value (Base Stat + Multiplier)
        double logMult = ability.Multiplier > 0 ? Math.Log10(ability.Multiplier) : double.NegativeInfinity;
        double baseStat = (ability.ScaleStat == AbilityScaleStat.Health) ? caster.MaxHp : caster.Dmg;
        double finalValue = baseStat + logMult;

        if (ability.Effect == AbilityEffect.Heal)
        {
            for (int i = 0; i < actualCount; i++)
                Heal(ref targets[selectedIndices[i]], finalValue);
        }
        else // Damage
        {
            for (int i = 0; i < actualCount; i++)
            {
                ref BattleMember t = ref targets[selectedIndices[i]];
                if (Damage(ref t, DamageTaken(finalValue, t.Arm)))
                    aliveEnemies--;
            }
        }
    }

    private static int SelectTargets(Span<BattleMember> team, Ability ability, int casterIndex, Span<int> output)
    {
        // Filter alive units into a temporary stack-allocated buffer
        Span<int> aliveIndices = stackalloc int[team.Length];
        int aliveCount = 0;
        for (int i = 0; i < team.Length; i++)
        {
            if (!team[i].IsDead)
                aliveIndices[aliveCount++] = i;
        }

        if (aliveCount == 0) return 0;

        int numToTake = Math.Min(ability.NumTargets, aliveCount);

        switch (ability.TargetPosition)
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
                // Backline priority: Pick from the end of the alive list
                for (int i = 0; i < numToTake; i++)
                    output[i] = aliveIndices[aliveCount - 1 - i];
                return numToTake;

            case AbilityTargetPosition.Random:
                // Fisher-Yates shuffle
                for (int i = 0; i < numToTake; i++)
                {
                    int j = Random.Shared.Next(i, aliveCount);
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

/// <summary>
/// Mutable battle state for one slot — lives only inside a Run call.
/// Struct keeps the team arrays inline (no per-slot heap allocation).
/// </summary>
internal struct BattleMember(ComputedMachine? source, double hp, double dmg, double arm)
{
    public readonly ComputedMachine? Source = source;
    public bool IsDead = false;

    public double Hp = hp;
    public double MaxHp = hp;
    public double Dmg = dmg;
    public double Arm = arm;
}

public sealed record BattleResult(bool PlayerWon, int Rounds);