using InfatalsFirestoneTools.Models;
using InfatalsFirestoneTools.Services;

namespace InfatalsFirestoneTools.Tests.Services;

public class ArtifactServiceTests
{
    private readonly ArtifactService _sut = new();

    // ── CreateArtifacts ───────────────────────────────────────────────────────

    [Fact]
    public void CreateArtifacts_ReturnsCorrectCount()
    {
        // 3 stats × 8 tiers = 24
        List<Artifact> artifacts = _sut.CreateArtifacts();
        Assert.Equal(24, artifacts.Count);
    }

    [Fact]
    public void CreateArtifacts_AllCountsAreZero()
    {
        List<Artifact> artifacts = _sut.CreateArtifacts();
        Assert.All(artifacts, a => Assert.Equal(0, a.Count));
    }

    [Fact]
    public void CreateArtifacts_ContainsAllStats()
    {
        List<Artifact> artifacts = _sut.CreateArtifacts();
        var stats = artifacts.Select(a => a.Stat).Distinct().ToList();

        Assert.Contains(ArtifactStat.Damage, stats);
        Assert.Contains(ArtifactStat.Health, stats);
        Assert.Contains(ArtifactStat.Armor, stats);
    }

    [Fact]
    public void CreateArtifacts_ContainsAllTiers()
    {
        List<Artifact> artifacts = _sut.CreateArtifacts();
        var percentages = artifacts.Select(a => a.Percentage).Distinct().OrderBy(x => x).ToList();

        Assert.Equal(ArtifactService.Tiers, percentages);
    }

    [Fact]
    public void CreateArtifacts_EachStatHasAllTiers()
    {
        List<Artifact> artifacts = _sut.CreateArtifacts();

        foreach (ArtifactStat stat in Enum.GetValues<ArtifactStat>())
        {
            var forStat = artifacts.Where(a => a.Stat == stat).Select(a => a.Percentage).OrderBy(x => x).ToList();
            Assert.Equal(ArtifactService.Tiers, forStat);
        }
    }

    // ── GetByStat ─────────────────────────────────────────────────────────────

    [Fact]
    public void GetByStat_Damage_ReturnsOnlyDamageArtifacts()
    {
        List<Artifact> artifacts = _sut.CreateArtifacts();
        var damage = ArtifactService.GetByStat(artifacts, ArtifactStat.Damage).ToList();

        Assert.Equal(ArtifactService.Tiers.Length, damage.Count);
        Assert.All(damage, a => Assert.Equal(ArtifactStat.Damage, a.Stat));
    }

    // ── ResetAll ──────────────────────────────────────────────────────────────

    [Fact]
    public void ResetAll_SetsAllCountsToZero()
    {
        List<Artifact> artifacts = _sut.CreateArtifacts();
        foreach (Artifact a in artifacts) a.Count = 5;

        ArtifactService.ResetAll(artifacts);

        Assert.All(artifacts, a => Assert.Equal(0, a.Count));
    }

    // ── Tiers constant ────────────────────────────────────────────────────────

    [Fact]
    public void Tiers_AreInAscendingOrder()
    {
        var sorted = ArtifactService.Tiers.OrderBy(x => x).ToArray();
        Assert.Equal(sorted, ArtifactService.Tiers);
    }
}