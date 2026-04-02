using InfatalsFirestoneTools.Models;

namespace InfatalsFirestoneTools.Tests.Models;

public class GuardianRankExtensionsTests
{
    // ── IsStar / IsCrown ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(GuardianRank.OneStar)]
    [InlineData(GuardianRank.TwoStar)]
    [InlineData(GuardianRank.ThreeStar)]
    [InlineData(GuardianRank.FourStar)]
    [InlineData(GuardianRank.FiveStar)]
    public void IsStar_StarRanks_ReturnsTrue(GuardianRank rank)
    {
        Assert.True(rank.IsStar());
        Assert.False(rank.IsCrown());
    }

    [Theory]
    [InlineData(GuardianRank.OneCrown)]
    [InlineData(GuardianRank.TwoCrown)]
    [InlineData(GuardianRank.ThreeCrown)]
    [InlineData(GuardianRank.FourCrown)]
    [InlineData(GuardianRank.FiveCrown)]
    public void IsCrown_CrownRanks_ReturnsTrue(GuardianRank rank)
    {
        Assert.True(rank.IsCrown());
        Assert.False(rank.IsStar());
    }

    // ── StarCount ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(GuardianRank.OneStar, 1)]
    [InlineData(GuardianRank.TwoStar, 2)]
    [InlineData(GuardianRank.ThreeStar, 3)]
    [InlineData(GuardianRank.FourStar, 4)]
    [InlineData(GuardianRank.FiveStar, 5)]
    public void StarCount_StarRanks_ReturnsCorrectCount(GuardianRank rank, int expected)
    {
        Assert.Equal(expected, rank.StarCount());
    }

    [Theory]
    [InlineData(GuardianRank.OneCrown)]
    [InlineData(GuardianRank.FiveCrown)]
    public void StarCount_CrownRanks_ReturnsZero(GuardianRank rank)
    {
        Assert.Equal(0, rank.StarCount());
    }

    // ── CrownCount ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(GuardianRank.OneCrown, 1)]
    [InlineData(GuardianRank.TwoCrown, 2)]
    [InlineData(GuardianRank.ThreeCrown, 3)]
    [InlineData(GuardianRank.FourCrown, 4)]
    [InlineData(GuardianRank.FiveCrown, 5)]
    public void CrownCount_CrownRanks_ReturnsCorrectCount(GuardianRank rank, int expected)
    {
        Assert.Equal(expected, rank.CrownCount());
    }

    [Theory]
    [InlineData(GuardianRank.OneStar)]
    [InlineData(GuardianRank.FiveStar)]
    public void CrownCount_StarRanks_ReturnsZero(GuardianRank rank)
    {
        Assert.Equal(0, rank.CrownCount());
    }

    // ── RankIconPath ──────────────────────────────────────────────────────────

    [Fact]
    public void RankIconPath_BronzeOneStar_ReturnsBronzePath()
    {
        string path = GuardianRank.OneStar.RankIconPath(GuardianEvolution.Bronze);
        Assert.Contains("star1Bronze", path);
        Assert.StartsWith("img/ui/ranks/", path);
    }

    [Fact]
    public void RankIconPath_BronzeOneCrown_ReturnsBronzeCrownPath()
    {
        string path = GuardianRank.OneCrown.RankIconPath(GuardianEvolution.Bronze);
        Assert.Contains("crown11Bronze", path);
    }

    [Fact]
    public void RankIconPath_SapphireEvo_ContainsSepphireTypo()
    {
        // The game files have a typo "Sepphire" — the code must mirror it exactly
        string starPath = GuardianRank.OneStar.RankIconPath(GuardianEvolution.Sapphire);
        string crownPath = GuardianRank.OneCrown.RankIconPath(GuardianEvolution.Sapphire);
        Assert.Contains("Sepphire", starPath);
        Assert.Contains("Sepphire", crownPath);
    }

    [Theory]
    [InlineData(GuardianEvolution.Silver, "star2Silver")]
    [InlineData(GuardianEvolution.Gold, "star3Gold")]
    [InlineData(GuardianEvolution.Platinum, "star4Platinum")]
    [InlineData(GuardianEvolution.Ruby, "star5Ruby")]
    [InlineData(GuardianEvolution.Pearl, "star7Pearl")]
    [InlineData(GuardianEvolution.Diamond, "star8Diamond")]
    [InlineData(GuardianEvolution.Starlight, "star9Starlight")]
    [InlineData(GuardianEvolution.StarlightPlus, "star10StarlightPlus")]
    public void RankIconPath_StarEvolutions_ReturnCorrectFileName(GuardianEvolution evo, string expectedFile)
    {
        string path = GuardianRank.OneStar.RankIconPath(evo);
        Assert.Contains(expectedFile, path);
    }
}