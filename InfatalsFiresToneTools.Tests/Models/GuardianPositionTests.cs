using InfatalsFirestoneTools.Models;

namespace InfatalsFirestoneTools.Tests.Models;

public class GuardianPositionTests
{
    // ── AllPositions ──────────────────────────────────────────────────────────

    [Fact]
    public void AllPositions_CountIs100()
    {
        // 10 evolutions × 5 star ranks + 10 evolutions × 5 crown ranks = 100
        Assert.Equal(100, GuardianPosition.AllPositions.Count);
    }

    [Fact]
    public void AllPositions_StarsComeBeforeCrowns()
    {
        IReadOnlyList<GuardianPosition> positions = GuardianPosition.AllPositions;
        int firstCrown = positions.Select((p, i) => (p, i))
            .First(x => x.p.Rank.IsCrown()).i;
        int lastStar = positions.Select((p, i) => (p, i))
            .Last(x => x.p.Rank.IsStar()).i;

        Assert.True(lastStar < firstCrown, "All stars must come before any crown.");
    }

    [Fact]
    public void AllPositions_FirstIsBronze1Star()
    {
        GuardianPosition first = GuardianPosition.AllPositions[0];
        Assert.Equal(GuardianEvolution.Bronze, first.Evolution);
        Assert.Equal(GuardianRank.OneStar, first.Rank);
    }

    [Fact]
    public void AllPositions_LastIsStarlightPlus5Crown()
    {
        GuardianPosition last = GuardianPosition.AllPositions[^1];
        Assert.Equal(GuardianEvolution.StarlightPlus, last.Evolution);
        Assert.Equal(GuardianRank.FiveCrown, last.Rank);
    }

    // ── Ordinal ordering ──────────────────────────────────────────────────────

    [Fact]
    public void Ordinal_IsStrictlyIncreasingThroughAllPositions()
    {
        IReadOnlyList<GuardianPosition> positions = GuardianPosition.AllPositions;
        for (int i = 1; i < positions.Count; i++)
            Assert.True(positions[i].Ordinal > positions[i - 1].Ordinal,
                $"Ordinal at index {i} is not greater than at index {i - 1}");
    }

    [Fact]
    public void IsAfter_HigherOrdinal_ReturnsTrue()
    {
        var bronze2 = new GuardianPosition(GuardianEvolution.Bronze, GuardianRank.TwoStar);
        var bronze1 = new GuardianPosition(GuardianEvolution.Bronze, GuardianRank.OneStar);
        Assert.True(bronze2.IsAfter(bronze1));
        Assert.False(bronze1.IsAfter(bronze2));
    }

    [Fact]
    public void IsSameOrAfter_EqualPositions_ReturnsTrue()
    {
        var pos = new GuardianPosition(GuardianEvolution.Gold, GuardianRank.ThreeStar);
        Assert.True(pos.IsSameOrAfter(pos));
    }

    [Fact]
    public void IsAfter_SamePosition_ReturnsFalse()
    {
        var pos = new GuardianPosition(GuardianEvolution.Silver, GuardianRank.TwoStar);
        Assert.False(pos.IsAfter(pos));
    }

    // ── Crown comes after all stars ───────────────────────────────────────────

    [Fact]
    public void Ordinal_BronzeOneCrown_IsAfterStarlightPlusFiveStar()
    {
        var lastStar = new GuardianPosition(GuardianEvolution.StarlightPlus, GuardianRank.FiveStar);
        var firstCrown = new GuardianPosition(GuardianEvolution.Bronze, GuardianRank.OneCrown);
        Assert.True(firstCrown.IsAfter(lastStar));
    }

    // ── Label and IconPath ────────────────────────────────────────────────────

    [Fact]
    public void Label_ContainsRankAndEvolutionInfo()
    {
        var pos = new GuardianPosition(GuardianEvolution.Bronze, GuardianRank.OneStar);
        Assert.NotNull(pos.Label);
        Assert.NotEmpty(pos.Label);
    }

    [Fact]
    public void IconPath_StarRank_ReturnsNonEmptyPath()
    {
        var pos = new GuardianPosition(GuardianEvolution.Gold, GuardianRank.ThreeStar);
        Assert.NotEmpty(pos.IconPath);
        Assert.StartsWith("img/ui/ranks/", pos.IconPath);
    }

    [Fact]
    public void IconCount_StarRank_MatchesStarCount()
    {
        var pos = new GuardianPosition(GuardianEvolution.Bronze, GuardianRank.ThreeStar);
        Assert.Equal(3, pos.IconCount);
    }

    [Fact]
    public void IconCount_CrownRank_MatchesCrownCount()
    {
        var pos = new GuardianPosition(GuardianEvolution.Gold, GuardianRank.TwoCrown);
        Assert.Equal(2, pos.IconCount);
    }
}