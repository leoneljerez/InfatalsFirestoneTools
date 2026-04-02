using InfatalsFirestoneTools.Models;
using InfatalsFirestoneTools.Services;

namespace InfatalsFirestoneTools.Tests.Services;

public class GuardianCalculatorServiceTests
{
    private static GuardianPosition Pos(GuardianEvolution evo, GuardianRank rank)
    {
        return new(evo, rank);
    }

    // ── Validate ──────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_SamePositionSameLevel_ReturnsError()
    {
        GuardianPosition pos = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        string? error = GuardianCalculatorService.Validate(pos, 1, 0, pos, 1);
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_SamePositionHigherTarget_ReturnsNull()
    {
        GuardianPosition pos = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        string? error = GuardianCalculatorService.Validate(pos, 1, 0, pos, 2);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_TargetLowerThanCurrent_ReturnsError()
    {
        GuardianPosition high = Pos(GuardianEvolution.Gold, GuardianRank.ThreeStar);
        GuardianPosition low = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        string? error = GuardianCalculatorService.Validate(high, 1, 0, low, 1);
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_CrownToStarBackwards_ReturnsError()
    {
        GuardianPosition crown = Pos(GuardianEvolution.Bronze, GuardianRank.OneCrown);
        GuardianPosition star = Pos(GuardianEvolution.Bronze, GuardianRank.FiveStar);
        string? error = GuardianCalculatorService.Validate(crown, 1, 0, star, 1);
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_InvalidCurrentLevel_ReturnsError()
    {
        GuardianPosition pos = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        GuardianPosition target = Pos(GuardianEvolution.Bronze, GuardianRank.TwoStar);
        string? error = GuardianCalculatorService.Validate(pos, 11, 0, target, 1);
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_InvalidTargetLevel_ReturnsError()
    {
        GuardianPosition current = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        GuardianPosition target = Pos(GuardianEvolution.Bronze, GuardianRank.TwoStar);
        string? error = GuardianCalculatorService.Validate(current, 1, 0, target, 0);
        Assert.NotNull(error);
    }

    [Fact]
    public void Validate_NegativeExp_ReturnsError()
    {
        GuardianPosition current = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        GuardianPosition target = Pos(GuardianEvolution.Bronze, GuardianRank.TwoStar);
        string? error = GuardianCalculatorService.Validate(current, 1, -1, target, 1);
        Assert.NotNull(error);
    }

    // ── ExpForLevel ───────────────────────────────────────────────────────────

    [Fact]
    public void ExpForLevel_Bronze1StarLevel1_Returns90()
    {
        int exp = GuardianCalculatorService.ExpForLevel(
            GuardianEvolution.Bronze, GuardianRank.OneStar, 1);
        Assert.Equal(90, exp);
    }

    [Fact]
    public void ExpForLevel_LevelOutOfRange_Throws()
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
            GuardianCalculatorService.ExpForLevel(GuardianEvolution.Bronze, GuardianRank.OneStar, 0));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() =>
            GuardianCalculatorService.ExpForLevel(GuardianEvolution.Bronze, GuardianRank.OneStar, 10));
    }

    // ── Calculate ─────────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_SamePosition_ReturnsError()
    {
        GuardianPosition pos = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        GuardianCalculatorService.CalcResult result = GuardianCalculatorService.Calculate(pos, 1, 0, pos, 1);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Calculate_ValidPath_ReturnsPositiveDust()
    {
        GuardianPosition current = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        GuardianPosition target = Pos(GuardianEvolution.Bronze, GuardianRank.TwoStar);
        GuardianCalculatorService.CalcResult result = GuardianCalculatorService.Calculate(current, 1, 0, target, 1);

        Assert.Null(result.Error);
        Assert.True(result.TotalStrangeDust > 0);
    }

    [Fact]
    public void Calculate_WithCurrentExp_ReducesDustNeeded()
    {
        GuardianPosition current = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        GuardianPosition target = Pos(GuardianEvolution.Bronze, GuardianRank.TwoStar);

        GuardianCalculatorService.CalcResult resultNoExp = GuardianCalculatorService.Calculate(current, 1, 0, target, 1);
        GuardianCalculatorService.CalcResult resultWithExp = GuardianCalculatorService.Calculate(current, 1, 500, target, 1);

        Assert.True(resultWithExp.StrangeDustForExp <= resultNoExp.StrangeDustForExp);
    }

    [Fact]
    public void Calculate_TotalEqualsSumOfParts()
    {
        GuardianPosition current = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        GuardianPosition target = Pos(GuardianEvolution.Silver, GuardianRank.OneStar);
        GuardianCalculatorService.CalcResult result = GuardianCalculatorService.Calculate(current, 1, 0, target, 1);

        Assert.Equal(
            result.StrangeDustForExp + result.StrangeDustForEvolutions,
            result.TotalStrangeDust);
    }

    [Fact]
    public void Calculate_SameRankHigherLevel_NoEvolutionCosts()
    {
        GuardianPosition current = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        GuardianPosition target = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        GuardianCalculatorService.CalcResult result = GuardianCalculatorService.Calculate(current, 1, 0, target, 5);

        Assert.Null(result.Error);
        Assert.Equal(0, result.StrangeDustForEvolutions);
        Assert.Empty(result.Evolutions);
    }

    [Fact]
    public void Calculate_DustIsMultipleOf20()
    {
        GuardianPosition current = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        GuardianPosition target = Pos(GuardianEvolution.Gold, GuardianRank.ThreeStar);
        GuardianCalculatorService.CalcResult result = GuardianCalculatorService.Calculate(current, 1, 0, target, 5);

        Assert.Equal(0, result.StrangeDustForExp % 20);
    }

    // ── CalculateEvolutions ───────────────────────────────────────────────────

    [Fact]
    public void CalculateEvolutions_Bronze1To2Star_ReturnsOneStep()
    {
        GuardianPosition current = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        GuardianPosition target = Pos(GuardianEvolution.Bronze, GuardianRank.TwoStar);
        IReadOnlyList<GuardianCalculatorService.EvolutionStep> evos = GuardianCalculatorService.CalculateEvolutions(current, target);

        _ = Assert.Single(evos);
        Assert.Equal(GuardianEvolution.Bronze, evos[0].Evolution);
    }

    [Fact]
    public void CalculateEvolutions_SamePosition_ReturnsEmpty()
    {
        GuardianPosition pos = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        IReadOnlyList<GuardianCalculatorService.EvolutionStep> evos = GuardianCalculatorService.CalculateEvolutions(pos, pos);
        Assert.Empty(evos);
    }

    [Fact]
    public void CalculateEvolutions_AcrossEvolutions_CostsArePositive()
    {
        GuardianPosition current = Pos(GuardianEvolution.Bronze, GuardianRank.FiveStar);
        GuardianPosition target = Pos(GuardianEvolution.Silver, GuardianRank.TwoStar);
        IReadOnlyList<GuardianCalculatorService.EvolutionStep> evos = GuardianCalculatorService.CalculateEvolutions(current, target);

        Assert.All(evos, step => Assert.True(step.Cost > 0));
    }

    [Fact]
    public void CalculateEvolutions_StarToCrown_IncludesAllIntermediateSteps()
    {
        GuardianPosition current = Pos(GuardianEvolution.Bronze, GuardianRank.OneStar);
        GuardianPosition target = Pos(GuardianEvolution.Bronze, GuardianRank.OneCrown);
        IReadOnlyList<GuardianCalculatorService.EvolutionStep> evos = GuardianCalculatorService.CalculateEvolutions(current, target);

        // Should cross Bronze 1→2→3→4→5 Star, then Bronze 5Star→1Crown = 5 steps
        Assert.Equal(50, evos.Count);
    }

    // ── Constants ─────────────────────────────────────────────────────────────

    [Fact]
    public void Constants_DustExpPerItem_Is120()
    {
        Assert.Equal(120, GuardianCalculatorService.StrangeDustExpPerItem);
    }

    [Fact]
    public void Constants_DustPerStack_Is20()
    {
        Assert.Equal(20, GuardianCalculatorService.StrangeDustPerStack);
    }
}