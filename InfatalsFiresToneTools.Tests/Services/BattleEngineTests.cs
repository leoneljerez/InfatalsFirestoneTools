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
        ComputedMachine computed = new(staticData, dynamic);
        computed.BattleStats = new MachineStats
        {
            Damage = new BigDouble(damage),
            Health = new BigDouble(health),
            Armor = new BigDouble(armor),
        };
        return computed;
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
        List<ComputedMachine> playerTeam = new()
        {
            MakeComputedMachine(1, damage: 1e12, health: 1e15, armor: 1e10),
        };
        var enemyTeam = new[] { MakeEnemyStats(1, 1, 0) };

        var result = _engine.Run(playerTeam, enemyTeam, enableAbilities: false);

        Assert.True(result.PlayerWon);
    }

    // ── Player loss scenarios ─────────────────────────────────────────────────

    [Fact]
    public void Run_VeryWeakPlayer_PlayerLoses()
    {
        List<ComputedMachine> playerTeam = new()
        {
            MakeComputedMachine(1, damage: 1, health: 1, armor: 0),
        };
        var enemyTeam = new[]
        {
            MakeEnemyStats(damage: 1e15, health: 1e15, armor: 1e14),
        };

        var result = _engine.Run(playerTeam, enemyTeam, enableAbilities: false);

        Assert.False(result.PlayerWon);
    }

    // ── Round counting ────────────────────────────────────────────────────────

    [Fact]
    public void Run_PlayerWinsRound0_WhenEnemyDiesInFirstHit()
    {
        List<ComputedMachine> playerTeam = new()
        {
            MakeComputedMachine(1, damage: 1e20, health: 1e20, armor: 0),
        };
        var enemyTeam = new[] { MakeEnemyStats(1, 1, 0) };

        var result = _engine.Run(playerTeam, enemyTeam, enableAbilities: false);

        Assert.True(result.PlayerWon);
        Assert.Equal(0, result.Rounds); // enemy dies before end of round 1
    }

    [Fact]
    public void Run_DoesNotExceedMaxRounds()
    {
        // Give both sides equal stats so battle drags on
        List<ComputedMachine> playerTeam = new()
        {
            MakeComputedMachine(1, damage: 10, health: 1e18, armor: 9),
        };
        var enemyTeam = new[] { MakeEnemyStats(damage: 10, health: 1e18, armor: 9) };

        var result = _engine.Run(playerTeam, enemyTeam, enableAbilities: false);

        Assert.True(result.Rounds <= 20);
    }

    // ── Armor interaction ─────────────────────────────────────────────────────

    [Fact]
    public void Run_HighEnemyArmor_BlocksPlayerDamage()
    {
        // Player damage equals enemy armor — no damage gets through
        List<ComputedMachine> playerTeam = new()
        {
            MakeComputedMachine(1, damage: 100, health: 1000, armor: 0),
        };
        var enemyTeam = new[] { MakeEnemyStats(damage: 1e15, health: 1e15, armor: 100) };

        var result = _engine.Run(playerTeam, enemyTeam, enableAbilities: false);

        Assert.False(result.PlayerWon); // player can't pierce armor
    }

    // ── Multiple team members ─────────────────────────────────────────────────

    [Fact]
    public void Run_FiveVsFive_CompletesWithoutException()
    {
        List<ComputedMachine> playerTeam = Enumerable.Range(1, 5)
            .Select(i => MakeComputedMachine(i, damage: 1000, health: 5000, armor: 50))
            .ToList();
        var enemyTeam = Enumerable.Range(1, 5)
            .Select(_ => MakeEnemyStats(500, 5000, 0))
            .ToArray();

        var ex = Record.Exception(() => _engine.Run(playerTeam, enemyTeam, enableAbilities: false));
        Assert.Null(ex);
    }

    // ── Empty teams ───────────────────────────────────────────────────────────

    [Fact]
    public void Run_EmptyPlayerTeam_PlayerLoses()
    {
        var enemyTeam = new[] { MakeEnemyStats(1, 1, 0) };
        var result = _engine.Run([], enemyTeam, enableAbilities: false);
        Assert.False(result.PlayerWon);
    }
}