using InfatalsFirestoneTools.Models;

namespace InfatalsFirestoneTools.Tests.Models;

public class MachineRankTests
{
    // ── FromLevel — null cases ────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void FromLevel_LevelZeroOrBelow_ReturnsNull(int level)
    {
        Assert.Null(MachineRank.FromLevel(level));
    }

    // ── FromLevel — Star phase (1–50) ─────────────────────────────────────────

    [Fact]
    public void FromLevel_Level1_IsBronze1Star()
    {
        MachineRank? rank = MachineRank.FromLevel(1);
        Assert.NotNull(rank);
        Assert.Equal(MachineLevelRankType.Star, rank.Type);
        Assert.Equal(MachineLevelRankTier.Bronze, rank.Tier);
        Assert.Equal(1, rank.Count);
    }

    [Fact]
    public void FromLevel_Level5_IsBronze5Stars()
    {
        MachineRank? rank = MachineRank.FromLevel(5);
        Assert.NotNull(rank);
        Assert.Equal(MachineLevelRankType.Star, rank.Type);
        Assert.Equal(MachineLevelRankTier.Bronze, rank.Tier);
        Assert.Equal(5, rank.Count);
    }

    [Fact]
    public void FromLevel_Level6_IsSilver1Star()
    {
        MachineRank? rank = MachineRank.FromLevel(6);
        Assert.NotNull(rank);
        Assert.Equal(MachineLevelRankType.Star, rank.Type);
        Assert.Equal(MachineLevelRankTier.Silver, rank.Tier);
        Assert.Equal(1, rank.Count);
    }

    [Fact]
    public void FromLevel_Level50_IsStarlightPlus5Stars()
    {
        MachineRank? rank = MachineRank.FromLevel(50);
        Assert.NotNull(rank);
        Assert.Equal(MachineLevelRankType.Star, rank.Type);
        Assert.Equal(5, rank.Count);
    }

    // ── FromLevel — Crown phase (51–100) ─────────────────────────────────────

    [Fact]
    public void FromLevel_Level51_IsBronze1Crown()
    {
        MachineRank? rank = MachineRank.FromLevel(51);
        Assert.NotNull(rank);
        Assert.Equal(MachineLevelRankType.Crown, rank.Type);
        Assert.Equal(MachineLevelRankTier.Bronze, rank.Tier);
        Assert.Equal(1, rank.Count);
    }

    [Fact]
    public void FromLevel_Level100_IsStarlightPlus5Crowns()
    {
        MachineRank? rank = MachineRank.FromLevel(100);
        Assert.NotNull(rank);
        Assert.Equal(MachineLevelRankType.Crown, rank.Type);
        Assert.Equal(5, rank.Count);
    }

    // ── FromLevel — Wings phase (101–150) ─────────────────────────────────────

    [Fact]
    public void FromLevel_Level101_IsBronze1Wings()
    {
        MachineRank? rank = MachineRank.FromLevel(101);
        Assert.NotNull(rank);
        Assert.Equal(MachineLevelRankType.Wings, rank.Type);
        Assert.Equal(MachineLevelRankTier.Bronze, rank.Tier);
        Assert.Equal(1, rank.Count);
    }

    [Fact]
    public void FromLevel_Level150_Is5StarlightPlusWings()
    {
        MachineRank? rank = MachineRank.FromLevel(150);
        Assert.NotNull(rank);
        Assert.Equal(MachineLevelRankType.Wings, rank.Type);
        Assert.Equal(5, rank.Count);
    }

    // ── FromLevel — Beyond 150 ───────────────────────────────────────────────

    [Fact]
    public void FromLevel_Level151_ReturnsMaxRank()
    {
        MachineRank? rank = MachineRank.FromLevel(151);
        Assert.NotNull(rank);
        Assert.Equal(MachineLevelRankType.Wings, rank.Type);
        Assert.Equal(MachineLevelRankTier.StarlightPlus, rank.Tier);
        Assert.Equal(5, rank.Count);
    }

    // ── ImageBasePath ─────────────────────────────────────────────────────────

    [Fact]
    public void ImageBasePath_StarBronze_ContainsStar1Bronze()
    {
        var rank = MachineRank.FromLevel(1)!;
        Assert.Contains("star1Bronze", rank.ImageBasePath);
        Assert.StartsWith("img/ui/ranks/", rank.ImageBasePath);
    }

    [Fact]
    public void ImageBasePath_CrownBronze_ContainsCrown11Bronze()
    {
        var rank = MachineRank.FromLevel(51)!;
        Assert.Contains("crown11Bronze", rank.ImageBasePath);
    }

    [Fact]
    public void ImageBasePath_WingsBronze_ContainsWings21Bronze()
    {
        var rank = MachineRank.FromLevel(101)!;
        Assert.Contains("wings21Bronze", rank.ImageBasePath);
    }

    // ── DisplayText ───────────────────────────────────────────────────────────

    [Fact]
    public void DisplayText_1BronzeStar_IsSingular()
    {
        var rank = MachineRank.FromLevel(1)!;
        Assert.DoesNotContain("Stars", rank.DisplayText); // "1 Bronze Star" not "Stars"
    }

    [Fact]
    public void DisplayText_2Stars_IsPlural()
    {
        var rank = MachineRank.FromLevel(2)!;
        Assert.Contains("Stars", rank.DisplayText);
    }
}