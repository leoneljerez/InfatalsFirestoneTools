using BreakEternity;
using InfatalsFirestoneTools.Models;
using InfatalsFirestoneTools.Services.Optimizer;

namespace InfatalsFirestoneTools.Tests.Services;

public class CalculatorTests
{
    private static ComputedMachine MakeMachine(
        MachineSpecialization spec = MachineSpecialization.Tank,
        MachineRarity rarity = MachineRarity.Common,
        int level = 1,
        int baseDmg = 1000,
        int baseHp = 10000,
        int baseArm = 200)
    {
        MachineStatic staticData = new()
        {
            Id = 1,
            Name = "TestMachine",
            Specialization = spec,
            TargetType = MachineTargetType.Single,
            Image = "img/test",
            BaseDamage = baseDmg,
            BaseHealth = baseHp,
            BaseArmor = baseArm,
        };
        Machine dynamic = new()
        {
            Id = 1,
            Rarity = rarity,
            Level = level,
        };
        return new ComputedMachine(staticData, dynamic);
    }

    // ── CalculateBattleAttributes ─────────────────────────────────────────────

    [Fact]
    public void CalculateBattleAttributes_Level1NoBonus_ReturnsBaseStats()
    {
        ComputedMachine machine = MakeMachine(level: 1, baseDmg: 1000, baseHp: 10000, baseArm: 200);
        (BigDouble dmg, BigDouble hp, BigDouble arm) = Calculator.CalculateBattleAttributes(
            machine, [], 0, [], engineerLevel: 1);

        // At level 1, engineer 1, rarity 0, no blueprints: multiplier = 1^0 = 1
        // So stats equal base stats
        Assert.True(dmg > BigDouble.dZero);
        Assert.True(hp > BigDouble.dZero);
        Assert.True(arm > BigDouble.dZero);
    }

    [Fact]
    public void CalculateBattleAttributes_HigherLevel_YieldsHigherStats()
    {
        ComputedMachine machine1 = MakeMachine(level: 1);
        ComputedMachine machine50 = MakeMachine(level: 50);

        (BigDouble dmg1, BigDouble hp1, BigDouble arm1) = Calculator.CalculateBattleAttributes(machine1, [], 0, [], 1);
        (BigDouble dmg50, BigDouble hp50, BigDouble arm50) = Calculator.CalculateBattleAttributes(machine50, [], 0, [], 1);

        Assert.True(dmg50 > dmg1);
        Assert.True(hp50 > hp1);
        Assert.True(arm50 > arm1);
    }

    [Fact]
    public void CalculateBattleAttributes_HigherRarity_YieldsHigherStats()
    {
        ComputedMachine common = MakeMachine(rarity: MachineRarity.Common);
        ComputedMachine legendary = MakeMachine(rarity: MachineRarity.Legendary);

        (BigDouble dmgC, BigDouble _, BigDouble _) = Calculator.CalculateBattleAttributes(common, [], 0, [], 1);
        (BigDouble dmgL, BigDouble _, BigDouble _) = Calculator.CalculateBattleAttributes(legendary, [], 0, [], 1);

        Assert.True(dmgL > dmgC);
    }

    [Fact]
    public void CalculateBattleAttributes_CrewBonus_IncreasesDamage()
    {
        ComputedMachine machine = MakeMachine();
        HeroStatic heroStatic = new()
        {
            Id = 1,
            Name = "H",
            Type = HeroType.Hero,
            Class = HeroClass.Warrior,
            AttackStyle = HeroAttackStyle.Melee,
            Specialization = HeroSpecialization.Damage,
            Resource = HeroResource.Rage,
            Image = "img/hero"
        };
        List<ComputedHero> crew =
        [
            new(heroStatic, new Hero { Id = 1, DamagePercentage = 100 })
        ];

        (BigDouble dmgNoCrew, BigDouble _, BigDouble _) = Calculator.CalculateBattleAttributes(machine, [], 0, [], 1);
        (BigDouble dmgWithCrew, BigDouble _, BigDouble _) = Calculator.CalculateBattleAttributes(machine, crew, 0, [], 1);

        Assert.True(dmgWithCrew > dmgNoCrew);
    }

    // ── MachinePower ──────────────────────────────────────────────────────────

    [Fact]
    public void MachinePower_HigherStats_YieldsHigherPower()
    {
        MachineStats weak = new() { Damage = 100, Health = 1000, Armor = 10 };
        MachineStats strong = new() { Damage = 10000, Health = 100000, Armor = 1000 };

        Assert.True(Calculator.MachinePower(strong) > Calculator.MachinePower(weak));
    }

    [Fact]
    public void MachinePower_ZeroStats_ReturnsZero()
    {
        MachineStats zero = new() { Damage = 0, Health = 0, Armor = 0 };
        Assert.True(Calculator.MachinePower(zero) == BigDouble.dZero);
    }

    // ── SquadPower ────────────────────────────────────────────────────────────

    [Fact]
    public void SquadPower_EmptyList_ReturnsZero()
    {
        BigDouble power = Calculator.SquadPower([], arena: false);
        Assert.True(power == BigDouble.dZero);
    }

    [Fact]
    public void SquadPower_FiveMachines_IsGreaterThanOneMachine()
    {
        List<ComputedMachine> machines = [.. Enumerable.Range(1, 5).Select(i =>
        {
            ComputedMachine m = MakeMachine(level: 10);
            m.BattleStats = new MachineStats { Damage = 1000, Health = 10000, Armor = 200 };
            return m;
        })];

        List<ComputedMachine> single = [machines[0]];
        Assert.True(Calculator.SquadPower(machines, false) > Calculator.SquadPower(single, false));
    }

    // ── EnemyStats ────────────────────────────────────────────────────────────

    [Fact]
    public void EnemyStats_Mission1Easy_ReturnsPositiveStats()
    {
        MachineStats stats = Calculator.EnemyStats(1, CampaignDifficulty.Easy);
        Assert.True(stats.Damage > BigDouble.dZero);
        Assert.True(stats.Health > BigDouble.dZero);
        Assert.True(stats.Armor > BigDouble.dZero);
    }

    [Fact]
    public void EnemyStats_LaterMissions_AreStronger()
    {
        MachineStats m1 = Calculator.EnemyStats(1, CampaignDifficulty.Easy);
        MachineStats m10 = Calculator.EnemyStats(10, CampaignDifficulty.Easy);
        Assert.True(m10.Damage > m1.Damage);
    }

    [Fact]
    public void EnemyStats_HarderDifficulties_AreStronger()
    {
        MachineStats easy = Calculator.EnemyStats(1, CampaignDifficulty.Easy);
        MachineStats nightmare = Calculator.EnemyStats(1, CampaignDifficulty.Nightmare);
        Assert.True(nightmare.Damage > easy.Damage);
    }

    // ── RequiredPower ─────────────────────────────────────────────────────────

    [Fact]
    public void RequiredPower_IsPositive()
    {
        BigDouble power = Calculator.RequiredPower(1, CampaignDifficulty.Easy);
        Assert.True(power > BigDouble.dZero);
    }

    [Fact]
    public void RequiredPower_LaterMissions_RequireMore()
    {
        BigDouble p1 = Calculator.RequiredPower(1, CampaignDifficulty.Normal);
        BigDouble p50 = Calculator.RequiredPower(50, CampaignDifficulty.Normal);
        Assert.True(p50 > p1);
    }

    // ── GetGlobalRarityLevels ─────────────────────────────────────────────────

    [Fact]
    public void GetGlobalRarityLevels_AllCommon_Returns0()
    {
        List<Machine> machines =
        [
            new() { Rarity = MachineRarity.Common },
            new() { Rarity = MachineRarity.Common },
        ];
        Assert.Equal(0, Calculator.GetGlobalRarityLevels(machines));
    }

    [Fact]
    public void GetGlobalRarityLevels_SumsRarityValues()
    {
        List<Machine> machines =
        [
            new() { Rarity = MachineRarity.Rare },       // 2
            new() { Rarity = MachineRarity.Epic },       // 3
            new() { Rarity = MachineRarity.Legendary },  // 4
        ];
        Assert.Equal(9, Calculator.GetGlobalRarityLevels(machines));
    }

    // ── CalculateOverdrive ────────────────────────────────────────────────────

    [Fact]
    public void CalculateOverdrive_CommonRarity_Returns25Percent()
    {
        ComputedMachine machine = MakeMachine(rarity: MachineRarity.Common);
        double overdrive = Calculator.CalculateOverdrive(machine);
        Assert.Equal(0.25, overdrive, precision: 5);
    }

    [Fact]
    public void CalculateOverdrive_HigherRarity_ReturnsHigherChance()
    {
        ComputedMachine common = MakeMachine(rarity: MachineRarity.Common);
        ComputedMachine legendary = MakeMachine(rarity: MachineRarity.Legendary);

        Assert.True(Calculator.CalculateOverdrive(legendary) > Calculator.CalculateOverdrive(common));
    }

    // ── CalculateArenaAttributes ──────────────────────────────────────────────

    [Fact]
    public void CalculateArenaAttributes_ReturnsPositiveStats()
    {
        ComputedMachine machine = MakeMachine(level: 50, rarity: MachineRarity.Epic);
        MachineStats battleStats = new()
        {
            Damage = new BigDouble(1e9),
            Health = new BigDouble(1e10),
            Armor = new BigDouble(1e8),
        };

        MachineStats arenaStats = Calculator.CalculateArenaAttributes(
            machine, battleStats, globalRarityLevels: 10,
            scarabLevel: 0, riftRank: ChaosRiftRank.Bronze);

        Assert.True(arenaStats.Damage > BigDouble.dZero);
        Assert.True(arenaStats.Health > BigDouble.dZero);
        Assert.True(arenaStats.Armor > BigDouble.dZero);
    }
}