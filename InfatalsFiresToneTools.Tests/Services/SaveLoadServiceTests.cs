using InfatalsFirestoneTools.Models;
using InfatalsFirestoneTools.Services;

namespace InfatalsFirestoneTools.Tests.Services;

public class SaveLoadServiceTests
{
    private static OptimizerData BuildTestData()
    {
        return new OptimizerData
        {
            EngineerLevel = 42,
            ScarabLevel = 15,
            ChaosRiftRank = ChaosRiftRank.Ruby,
            OptimizeMode = OptimizeMode.Arena,
            Machines =
            [
                new Machine { Id = 1, Rarity = MachineRarity.Epic, Level = 30, DamageBlueprint = 5 },
                new Machine { Id = 2, Rarity = MachineRarity.Rare, Level = 20 },
            ],
            Heroes =
            [
                new Hero { Id = 1, DamagePercentage = 100, HealthPercentage = 60 },
                new Hero { Id = 2, ArmorPercentage = 40 },
            ],
            Artifacts =
            [
                new Artifact { Stat = ArtifactStat.Damage, Percentage = 30, Count = 3 },
                new Artifact { Stat = ArtifactStat.Health, Percentage = 40, Count = 1 },
            ],
            HeroWeights = new HeroWeights
            {
                CampaignTankDamage = 0.5,
                CampaignTankHealth = 10.0,
                ArenaDpsDamage = 8.0,
            }
        };
    }

    // ── Export ────────────────────────────────────────────────────────────────

    [Fact]
    public void Export_ReturnsValidJson()
    {
        OptimizerData data = BuildTestData();
        string json = SaveLoadService.Export(data);
        Assert.NotEmpty(json);
        Assert.True(System.Text.Json.JsonDocument.Parse(json) is not null);
    }

    [Fact]
    public void Export_ContainsVersion2()
    {
        OptimizerData data = BuildTestData();
        string json = SaveLoadService.Export(data);
        Assert.Contains("\"version\": 2", json);
    }

    [Fact]
    public void Export_ContainsEngineerLevel()
    {
        OptimizerData data = BuildTestData();
        string json = SaveLoadService.Export(data);
        Assert.Contains("42", json);
    }

    // ── Round-trip ────────────────────────────────────────────────────────────

    [Fact]
    public void RoundTrip_GeneralSettings_ArePreserved()
    {
        OptimizerData original = BuildTestData();
        string json = SaveLoadService.Export(original);

        var loaded = new OptimizerData
        {
            Machines = [new Machine { Id = 1 }, new Machine { Id = 2 }],
            Heroes = [new Hero { Id = 1 }, new Hero { Id = 2 }],
            Artifacts =
            [
                new Artifact { Stat = ArtifactStat.Damage, Percentage = 30 },
                new Artifact { Stat = ArtifactStat.Health, Percentage = 40 },
            ],
        };

        ImportResult result = SaveLoadService.Import(json, loaded);

        Assert.True(result.Success);
        Assert.Equal(42, loaded.EngineerLevel);
        Assert.Equal(15, loaded.ScarabLevel);
        Assert.Equal(ChaosRiftRank.Ruby, loaded.ChaosRiftRank);
        Assert.Equal(OptimizeMode.Arena, loaded.OptimizeMode);
    }

    [Fact]
    public void RoundTrip_MachineData_IsPreserved()
    {
        OptimizerData original = BuildTestData();
        string json = SaveLoadService.Export(original);

        var loaded = new OptimizerData
        {
            Machines = [new Machine { Id = 1 }, new Machine { Id = 2 }],
            Heroes = [],
            Artifacts = [],
        };

        _ = SaveLoadService.Import(json, loaded);

        Machine m1 = loaded.Machines.First(m => m.Id == 1);
        Assert.Equal(MachineRarity.Epic, m1.Rarity);
        Assert.Equal(30, m1.Level);
        Assert.Equal(5, m1.DamageBlueprint);
    }

    [Fact]
    public void RoundTrip_HeroData_IsPreserved()
    {
        OptimizerData original = BuildTestData();
        string json = SaveLoadService.Export(original);

        var loaded = new OptimizerData
        {
            Machines = [],
            Heroes = [new Hero { Id = 1 }, new Hero { Id = 2 }],
            Artifacts = [],
        };

        _ = SaveLoadService.Import(json, loaded);

        Hero h1 = loaded.Heroes.First(h => h.Id == 1);
        Assert.Equal(100, h1.DamagePercentage);
        Assert.Equal(60, h1.HealthPercentage);
    }

    [Fact]
    public void RoundTrip_ArtifactData_IsPreserved()
    {
        OptimizerData original = BuildTestData();
        string json = SaveLoadService.Export(original);

        var loaded = new OptimizerData
        {
            Machines = [],
            Heroes = [],
            Artifacts =
            [
                new Artifact { Stat = ArtifactStat.Damage, Percentage = 30 },
                new Artifact { Stat = ArtifactStat.Health, Percentage = 40 },
            ],
        };

        _ = SaveLoadService.Import(json, loaded);

        Artifact dmg = loaded.Artifacts.First(a => a.Stat == ArtifactStat.Damage && a.Percentage == 30);
        Assert.Equal(3, dmg.Count);
    }

    [Fact]
    public void RoundTrip_HeroWeights_ArePreserved()
    {
        OptimizerData original = BuildTestData();
        string json = SaveLoadService.Export(original);
        var loaded = new OptimizerData { Machines = [], Heroes = [], Artifacts = [] };

        _ = SaveLoadService.Import(json, loaded);

        Assert.Equal(0.5, loaded.HeroWeights.CampaignTankDamage);
        Assert.Equal(10.0, loaded.HeroWeights.CampaignTankHealth);
        Assert.Equal(8.0, loaded.HeroWeights.ArenaDpsDamage);
    }

    // ── Import errors ─────────────────────────────────────────────────────────

    [Fact]
    public void Import_EmptyString_ReturnsFailure()
    {
        var data = new OptimizerData { Machines = [], Heroes = [], Artifacts = [] };
        ImportResult result = SaveLoadService.Import("", data);
        Assert.False(result.Success);
    }

    [Fact]
    public void Import_InvalidJson_ReturnsFailure()
    {
        var data = new OptimizerData { Machines = [], Heroes = [], Artifacts = [] };
        ImportResult result = SaveLoadService.Import("{ not valid json !", data);
        Assert.False(result.Success);
    }

    [Fact]
    public void Import_WhitespaceOnly_ReturnsFailure()
    {
        var data = new OptimizerData { Machines = [], Heroes = [], Artifacts = [] };
        ImportResult result = SaveLoadService.Import("   ", data);
        Assert.False(result.Success);
    }

    [Fact]
    public void Import_UnknownVersion_ReturnsFailure()
    {
        var data = new OptimizerData { Machines = [], Heroes = [], Artifacts = [] };
        ImportResult result = SaveLoadService.Import("{\"version\":99}", data);
        Assert.False(result.Success);
    }

    // ── V1 legacy import ──────────────────────────────────────────────────────

    [Fact]
    public void Import_V1Format_IsHandledSuccessfully()
    {
        string v1Json = """
        {
          "general": { "engineerLevel": 10, "scarabLevel": 5, "riftRank": "Gold" },
          "machines": [{"id": 1, "rarity": "Rare", "level": 5}],
          "heroes": [],
          "artifacts": { "Damage": { "30": 2 } }
        }
        """;

        var data = new OptimizerData
        {
            Machines = [new Machine { Id = 1 }],
            Heroes = [],
            Artifacts = [new Artifact { Stat = ArtifactStat.Damage, Percentage = 30 }],
        };

        ImportResult result = SaveLoadService.Import(v1Json, data);

        Assert.True(result.Success);
        Assert.Equal(10, data.EngineerLevel);
        Assert.Equal(5, data.ScarabLevel);
        Assert.Equal(ChaosRiftRank.Gold, data.ChaosRiftRank);
        Assert.Equal(MachineRarity.Rare, data.Machines[0].Rarity);
        Assert.Equal(2, data.Artifacts[0].Count);
    }
}