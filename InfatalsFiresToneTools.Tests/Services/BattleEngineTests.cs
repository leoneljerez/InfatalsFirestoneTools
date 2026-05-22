using BreakEternity;
using InfatalsFirestoneTools.Models;
using InfatalsFirestoneTools.Services.Optimizer;

namespace InfatalsFirestoneTools.Tests.Services;

public class BattleEngineTests
{
    private static ComputedMachine MakeComputedMachine(
        int id = 1,
        double damage = 1000,
        double health = 10000,
        double armor = 0,
        MachineSpecialization spec = MachineSpecialization.Damage)
    {
        MachineStatic staticData = new()
        {
            Id = id,
            Name = $"Machine{id}",
            Specialization = spec,
            TargetType = MachineTargetType.Single,
            Image = "img/test",
            BaseDamage = (int)damage,
            BaseHealth = (int)health,
            BaseArmor = (int)armor,
        };
        Machine dynamic = new() { Id = id };
        ComputedMachine computed = new(staticData, dynamic)
        {
            BattleStats = new MachineStats
            {
                Damage = new BigDouble(damage),
                Health = new BigDouble(health),
                Armor = new BigDouble(armor),
            }
        };
        return computed;
    }

    // Helper to quickly build a BattleMember with logarithmic stats for testing
    private static BattleMember CreateBattleMember(ComputedMachine? source, MachineStats stats, bool isPlayer)
    {
        BattleMember member = new()
        {
            Hp = ToLog(stats.Health),
            MaxHp = ToLog(stats.Health),
            Dmg = ToLog(stats.Damage),
            Arm = ToLog(stats.Armor),
            IsDead = false
        };

        if (isPlayer && source?.Ability != null)
        {
            Ability a = source.Ability;
            member.HasAbility = true;
            member.OverdriveChance = Calculator.CalculateOverdrive(source);
            member.Effect = a.Effect;
            member.TargetType = a.TargetType;
            member.TargetPosition = a.TargetPosition;
            member.NumTargets = a.NumTargets;
            member.ScaleStat = a.ScaleStat;
            member.MultiplierLog = a.Multiplier > 0 ? Math.Log10(a.Multiplier) : double.NegativeInfinity;
        }
        else
        {
            member.HasAbility = false;
        }

        return member;
    }

    private static double ToLog(BigDouble value)
    {
        return value > 0 ? value.log10().toDouble() : double.NegativeInfinity;
    }

    private static MachineStats MakeEnemyStats(double damage, double health, double armor)
    {
        return new() { Damage = new BigDouble(damage), Health = new BigDouble(health), Armor = new BigDouble(armor) };
    }

    private readonly BattleEngine _engine = new();

    // ── Player win scenarios ───────────────────────────────────────────────────

    [Fact]
    public void Run_VeryStrongPlayer_PlayerWins()
    {
        List<ComputedMachine> playerTeam =
        [
            MakeComputedMachine(1, damage: 1e12, health: 1e15, armor: 1e10),
        ];
        MachineStats[] enemyTeam = [MakeEnemyStats(1, 1, 0)];

        Span<BattleMember> initialPlayers = stackalloc BattleMember[playerTeam.Count];
        for (int i = 0; i < playerTeam.Count; i++)
            initialPlayers[i] = CreateBattleMember(playerTeam[i], playerTeam[i].BattleStats, true);

        Span<BattleMember> initialEnemies = stackalloc BattleMember[enemyTeam.Length];
        for (int i = 0; i < enemyTeam.Length; i++)
            initialEnemies[i] = CreateBattleMember(null, enemyTeam[i], false);

        XorShiftState rng = new((uint)Environment.TickCount);
        bool result = _engine.RunBatch(initialPlayers, initialEnemies, 1, ref rng);

        Assert.True(result);
    }

    // ── Player loss scenarios ─────────────────────────────────────────────────

    [Fact]
    public void Run_VeryWeakPlayer_PlayerLoses()
    {
        List<ComputedMachine> playerTeam =
        [
            MakeComputedMachine(1, damage: 1, health: 1, armor: 0),
        ];
        MachineStats[] enemyTeam =
        [
            MakeEnemyStats(damage: 1e15, health: 1e15, armor: 1e14),
        ];

        Span<BattleMember> initialPlayers = stackalloc BattleMember[playerTeam.Count];
        for (int i = 0; i < playerTeam.Count; i++)
            initialPlayers[i] = CreateBattleMember(playerTeam[i], playerTeam[i].BattleStats, true);

        Span<BattleMember> initialEnemies = stackalloc BattleMember[enemyTeam.Length];
        for (int i = 0; i < enemyTeam.Length; i++)
            initialEnemies[i] = CreateBattleMember(null, enemyTeam[i], false);

        XorShiftState rng = new((uint)Environment.TickCount);
        BattleResult result = _engine.Run(initialPlayers, initialEnemies, ref rng);

        Assert.False(result.PlayerWon);
    }

    // ── Round counting ────────────────────────────────────────────────────────

    [Fact]
    public void Run_PlayerWinsRound0_WhenEnemyDiesInFirstHit()
    {
        List<ComputedMachine> playerTeam =
        [
            MakeComputedMachine(1, damage: 1e20, health: 1e20, armor: 0),
        ];
        MachineStats[] enemyTeam = [MakeEnemyStats(1, 1, 0)];


        Span<BattleMember> initialPlayers = stackalloc BattleMember[playerTeam.Count];
        for (int i = 0; i < playerTeam.Count; i++)
            initialPlayers[i] = CreateBattleMember(playerTeam[i], playerTeam[i].BattleStats, true);

        Span<BattleMember> initialEnemies = stackalloc BattleMember[enemyTeam.Length];
        for (int i = 0; i < enemyTeam.Length; i++)
            initialEnemies[i] = CreateBattleMember(null, enemyTeam[i], false);

        XorShiftState rng = new((uint)Environment.TickCount);
        BattleResult result = _engine.Run(initialPlayers, initialEnemies, ref rng);

        Assert.True(result.PlayerWon);
        Assert.Equal(0, result.Rounds); // enemy dies before end of round 1
    }

    [Fact]
    public void Run_DoesNotExceedMaxRounds()
    {
        // Give both sides equal stats so battle drags on
        List<ComputedMachine> playerTeam =
        [
            MakeComputedMachine(1, damage: 10, health: 1e18, armor: 9),
        ];
        MachineStats[] enemyTeam = [MakeEnemyStats(damage: 10, health: 1e18, armor: 9)];

        Span<BattleMember> initialPlayers = stackalloc BattleMember[playerTeam.Count];
        for (int i = 0; i < playerTeam.Count; i++)
            initialPlayers[i] = CreateBattleMember(playerTeam[i], playerTeam[i].BattleStats, true);

        Span<BattleMember> initialEnemies = stackalloc BattleMember[enemyTeam.Length];
        for (int i = 0; i < enemyTeam.Length; i++)
            initialEnemies[i] = CreateBattleMember(null, enemyTeam[i], false);

        XorShiftState rng = new((uint)Environment.TickCount);
        BattleResult result = _engine.Run(initialPlayers, initialEnemies, ref rng);

        Assert.True(result.Rounds <= 20);
    }

    // ── Armor interaction ─────────────────────────────────────────────────────

    [Fact]
    public void Run_HighEnemyArmor_BlocksPlayerDamage()
    {
        // Player damage equals enemy armor — no damage gets through
        List<ComputedMachine> playerTeam =
        [
            MakeComputedMachine(1, damage: 100, health: 1000, armor: 0),
        ];
        MachineStats[] enemyTeam = [MakeEnemyStats(damage: 1e15, health: 1e15, armor: 100)];

        Span<BattleMember> initialPlayers = stackalloc BattleMember[playerTeam.Count];
        for (int i = 0; i < playerTeam.Count; i++)
            initialPlayers[i] = CreateBattleMember(playerTeam[i], playerTeam[i].BattleStats, true);

        Span<BattleMember> initialEnemies = stackalloc BattleMember[enemyTeam.Length];
        for (int i = 0; i < enemyTeam.Length; i++)
            initialEnemies[i] = CreateBattleMember(null, enemyTeam[i], false);

        XorShiftState rng = new((uint)Environment.TickCount);
        BattleResult result = _engine.Run(initialPlayers, initialEnemies, ref rng);

        Assert.False(result.PlayerWon); // player can't pierce armor
    }

    // ── Multiple team members ─────────────────────────────────────────────────

    [Fact]
    public void Run_FiveVsFive_CompletesWithoutException()
    {
        List<ComputedMachine> playerTeam = [.. Enumerable.Range(1, 5).Select(i => MakeComputedMachine(i, damage: 1000, health: 5000, armor: 50))];
        MachineStats[] enemyTeam = [.. Enumerable.Range(1, 5).Select(_ => MakeEnemyStats(500, 5000, 0))];

        Span<BattleMember> initialPlayers = stackalloc BattleMember[playerTeam.Count];
        for (int i = 0; i < playerTeam.Count; i++)
            initialPlayers[i] = CreateBattleMember(playerTeam[i], playerTeam[i].BattleStats, true);

        Span<BattleMember> initialEnemies = stackalloc BattleMember[enemyTeam.Length];
        for (int i = 0; i < enemyTeam.Length; i++)
            initialEnemies[i] = CreateBattleMember(null, enemyTeam[i], false);

        XorShiftState rng = new((uint)Environment.TickCount);

        Exception? ex = null;
        try
        {
            _ = _engine.RunBatch(initialPlayers, initialEnemies, runs: 1, ref rng);
        }
        catch (Exception e)
        {
            ex = e;
        }

        Assert.Null(ex);
    }

    // ── Empty teams ───────────────────────────────────────────────────────────

    [Fact]
    public void Run_EmptyPlayerTeam_PlayerLoses()
    {
        MachineStats[] enemyTeam = [MakeEnemyStats(1, 1, 0)];

        Span<BattleMember> initialEnemies = stackalloc BattleMember[enemyTeam.Length];
        for (int i = 0; i < enemyTeam.Length; i++)
            initialEnemies[i] = CreateBattleMember(null, enemyTeam[i], false);

        XorShiftState rng = new((uint)Environment.TickCount);

        BattleResult result = _engine.Run([], initialEnemies, ref rng);
        Assert.False(result.PlayerWon);
    }
}