using InfatalsFirestoneTools.Models;

namespace InfatalsFirestoneTools.Tests.Models;

public class OptimizerDataTests
{
    [Fact]
    public void NewOptimizerData_HasEmptyCollectionsByDefault()
    {
        OptimizerData data = new();
        Assert.Empty(data.Machines);
        Assert.Empty(data.Heroes);
        Assert.Empty(data.Artifacts);
    }

    [Fact]
    public void NewOptimizerData_HasBronzeRiftRankByDefault()
    {
        OptimizerData data = new();
        Assert.Equal(ChaosRiftRank.Bronze, data.ChaosRiftRank);
    }

    [Fact]
    public void NewOptimizerData_HasCampaignModeByDefault()
    {
        OptimizerData data = new();
        Assert.Equal(OptimizeMode.Campaign, data.OptimizeMode);
    }

    [Fact]
    public void NewOptimizerData_HeroWeightsAreNotNull()
    {
        OptimizerData data = new();
        Assert.NotNull(data.HeroWeights);
    }
}

public class HeroWeightsTests
{
    [Fact]
    public void DefaultWeights_CampaignTankHealthIsHighest()
    {
        HeroWeights weights = new();
        // Tank health (12.0) should dominate for campaign tanks
        Assert.True(weights.CampaignTankHealth > weights.CampaignTankDamage);
        Assert.True(weights.CampaignTankHealth > weights.CampaignTankArmor);
    }

    [Fact]
    public void DefaultWeights_CampaignDpsDamageIsHighest()
    {
        HeroWeights weights = new();
        // Damage (15.0) should dominate for campaign DPS
        Assert.True(weights.CampaignDpsDamage > weights.CampaignDpsHealth);
        Assert.True(weights.CampaignDpsDamage > weights.CampaignDpsArmor);
    }

    [Fact]
    public void DefaultWeights_AllValuesArePositive()
    {
        HeroWeights weights = new();
        Assert.True(weights.CampaignTankDamage > 0);
        Assert.True(weights.CampaignTankHealth > 0);
        Assert.True(weights.CampaignTankArmor > 0);
        Assert.True(weights.CampaignDpsDamage > 0);
        Assert.True(weights.CampaignDpsHealth > 0);
        Assert.True(weights.CampaignDpsArmor > 0);
        Assert.True(weights.ArenaTankDamage > 0);
        Assert.True(weights.ArenaTankHealth > 0);
        Assert.True(weights.ArenaTankArmor > 0);
        Assert.True(weights.ArenaDpsDamage > 0);
        Assert.True(weights.ArenaDpsHealth > 0);
        Assert.True(weights.ArenaDpsArmor > 0);
    }
}

public class MachineTests
{
    [Fact]
    public void NewMachine_HasCommonRarityByDefault()
    {
        Machine machine = new();
        Assert.Equal(MachineRarity.Common, machine.Rarity);
    }

    [Fact]
    public void NewMachine_HasZeroLevelAndBlueprints()
    {
        Machine machine = new();
        Assert.Equal(0, machine.Level);
        Assert.Equal(0, machine.DamageBlueprint);
        Assert.Equal(0, machine.HealthBlueprint);
        Assert.Equal(0, machine.ArmorBlueprint);
        Assert.Equal(0, machine.SacredLevel);
        Assert.Equal(0, machine.InscriptionLevel);
    }
}

public class HeroTests
{
    [Fact]
    public void NewHero_HasZeroPercentages()
    {
        Hero hero = new();
        Assert.Equal(0, hero.DamagePercentage);
        Assert.Equal(0, hero.HealthPercentage);
        Assert.Equal(0, hero.ArmorPercentage);
    }
}

public class ComputedMachineTests
{
    [Fact]
    public void ComputedMachine_ShortcutsMatchStaticData()
    {
        MachineStatic staticData = new()
        {
            Id = 42,
            Name = "TestLabel",
            Specialization = MachineSpecialization.Tank,
            TargetType = MachineTargetType.Multi,
            Image = "img/test",
            BaseDamage = 500,
            BaseHealth = 5000,
            BaseArmor = 100,
        };
        Machine dynamic = new() { Id = 42, Rarity = MachineRarity.Epic, Level = 25 };
        ComputedMachine computed = new(staticData, dynamic);

        Assert.Equal(42, computed.Id);
        Assert.Equal("TestLabel", computed.Name);
        Assert.Equal(MachineSpecialization.Tank, computed.Specialization);
        Assert.Equal(MachineTargetType.Multi, computed.TargetType);
        Assert.Equal(MachineRarity.Epic, computed.Rarity);
        Assert.Equal(25, computed.Level);
    }

    [Fact]
    public void ComputedMachine_CrewStartsEmpty()
    {
        MachineStatic staticData = new() { Id = 1, Image = "img/x", BaseDamage = 100, BaseHealth = 1000, BaseArmor = 10 };
        Machine dynamic = new() { Id = 1 };
        ComputedMachine computed = new(staticData, dynamic);

        Assert.Empty(computed.Crew);
    }
}

public class EnumCoverageTests
{
    // Ensure enum values have the expected integer mappings (game logic depends on ordinal)

    [Fact]
    public void MachineRarity_CommonIsZero()
    {
        Assert.Equal(0, (int)MachineRarity.Common);
    }

    [Fact]
    public void MachineRarity_CelestialIsHighest()
    {
        Assert.Equal(8, (int)MachineRarity.Celestial);
    }

    [Fact]
    public void GuardianRank_OneCrownIsFive()
    {
        // Crown starts at index 5 — critical for IsStar/IsCrown logic
        Assert.Equal(5, (int)GuardianRank.OneCrown);
    }

    [Fact]
    public void GuardianEvolution_BronzeIsZero()
    {
        Assert.Equal(0, (int)GuardianEvolution.Bronze);
    }

    [Fact]
    public void ChaosRiftRank_BronzeIsZero()
    {
        Assert.Equal(0, (int)ChaosRiftRank.Bronze);
    }
}