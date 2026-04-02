namespace InfatalsFirestoneTools.Services;

using InfatalsFirestoneTools.Models;
using InfatalsFirestoneTools.Resources;

public static class GuardianCalculatorService
{
    public const int StrangeDustExpPerItem = 120;
    public const int StrangeDustPerStack = 20;

    // ── EXP tables ────────────────────────────────────────────────────────────

    private static readonly Dictionary<GuardianEvolution, int> CategoryBases = new()
    {
        [GuardianEvolution.Bronze] = 5310,
        [GuardianEvolution.Silver] = 5810,
        [GuardianEvolution.Gold] = 6410,
        [GuardianEvolution.Platinum] = 6910,
        [GuardianEvolution.Ruby] = 7410,
        [GuardianEvolution.Sapphire] = 7910,
        [GuardianEvolution.Pearl] = 8610,
        [GuardianEvolution.Diamond] = 9110,
        [GuardianEvolution.Starlight] = 9610,
        [GuardianEvolution.StarlightPlus] = 10210,
    };

    private static readonly int[][] BronzeStarExpTable =
    [
        [90,   190,  300,  420,  580,  690,  780,  865,  950],
        [1357, 1459, 1561, 1660, 1770, 1860, 1960, 2070, 2180],
        [2800, 2910, 3030, 3150, 3270, 3390, 3510, 3600, 3750],
        [4400, 4700, 4830, 5000, 5140, 5280, 5420, 5560, 5700],
        [5710, 5720, 5730, 5740, 5750, 5760, 5770, 5780, 5800],
    ];

    private static readonly Dictionary<GuardianEvolution,
        Dictionary<GuardianRank, int>> EvolutionCosts = new()
        {
            [GuardianEvolution.Bronze] = new()
            {
                [GuardianRank.OneStar] = 300,
                [GuardianRank.TwoStar] = 500,
                [GuardianRank.ThreeStar] = 600,
                [GuardianRank.FourStar] = 800,
                [GuardianRank.FiveStar] = 1000,
                [GuardianRank.OneCrown] = 800,
                [GuardianRank.TwoCrown] = 1000,
                [GuardianRank.ThreeCrown] = 1150,
                [GuardianRank.FourCrown] = 1300,
                [GuardianRank.FiveCrown] = 1500,
            },
            [GuardianEvolution.Silver] = new()
            {
                [GuardianRank.OneStar] = 350,
                [GuardianRank.TwoStar] = 550,
                [GuardianRank.ThreeStar] = 650,
                [GuardianRank.FourStar] = 850,
                [GuardianRank.FiveStar] = 1050,
                [GuardianRank.OneCrown] = 850,
                [GuardianRank.TwoCrown] = 1050,
                [GuardianRank.ThreeCrown] = 1200,
                [GuardianRank.FourCrown] = 1350,
                [GuardianRank.FiveCrown] = 1550,
            },
            [GuardianEvolution.Gold] = new()
            {
                [GuardianRank.OneStar] = 400,
                [GuardianRank.TwoStar] = 600,
                [GuardianRank.ThreeStar] = 700,
                [GuardianRank.FourStar] = 900,
                [GuardianRank.FiveStar] = 1100,
                [GuardianRank.OneCrown] = 900,
                [GuardianRank.TwoCrown] = 1100,
                [GuardianRank.ThreeCrown] = 1250,
                [GuardianRank.FourCrown] = 1400,
                [GuardianRank.FiveCrown] = 1600,
            },
            [GuardianEvolution.Platinum] = new()
            {
                [GuardianRank.OneStar] = 450,
                [GuardianRank.TwoStar] = 650,
                [GuardianRank.ThreeStar] = 750,
                [GuardianRank.FourStar] = 950,
                [GuardianRank.FiveStar] = 1150,
                [GuardianRank.OneCrown] = 950,
                [GuardianRank.TwoCrown] = 1150,
                [GuardianRank.ThreeCrown] = 1300,
                [GuardianRank.FourCrown] = 1450,
                [GuardianRank.FiveCrown] = 1650,
            },
            [GuardianEvolution.Ruby] = new()
            {
                [GuardianRank.OneStar] = 500,
                [GuardianRank.TwoStar] = 700,
                [GuardianRank.ThreeStar] = 800,
                [GuardianRank.FourStar] = 1000,
                [GuardianRank.FiveStar] = 1200,
                [GuardianRank.OneCrown] = 1050,
                [GuardianRank.TwoCrown] = 1200,
                [GuardianRank.ThreeCrown] = 1350,
                [GuardianRank.FourCrown] = 1500,
                [GuardianRank.FiveCrown] = 1700,
            },
            [GuardianEvolution.Sapphire] = new()
            {
                [GuardianRank.OneStar] = 550,
                [GuardianRank.TwoStar] = 750,
                [GuardianRank.ThreeStar] = 900,
                [GuardianRank.FourStar] = 1050,
                [GuardianRank.FiveStar] = 1250,
                [GuardianRank.OneCrown] = 1100,
                [GuardianRank.TwoCrown] = 1250,
                [GuardianRank.ThreeCrown] = 1400,
                [GuardianRank.FourCrown] = 1550,
                [GuardianRank.FiveCrown] = 1750,
            },
            [GuardianEvolution.Pearl] = new()
            {
                [GuardianRank.OneStar] = 600,
                [GuardianRank.TwoStar] = 800,
                [GuardianRank.ThreeStar] = 950,
                [GuardianRank.FourStar] = 1100,
                [GuardianRank.FiveStar] = 1300,
                [GuardianRank.OneCrown] = 1150,
                [GuardianRank.TwoCrown] = 1300,
                [GuardianRank.ThreeCrown] = 1450,
                [GuardianRank.FourCrown] = 1600,
                [GuardianRank.FiveCrown] = 1800,
            },
            [GuardianEvolution.Diamond] = new()
            {
                [GuardianRank.OneStar] = 650,
                [GuardianRank.TwoStar] = 850,
                [GuardianRank.ThreeStar] = 1000,
                [GuardianRank.FourStar] = 1150,
                [GuardianRank.FiveStar] = 1350,
                [GuardianRank.OneCrown] = 1200,
                [GuardianRank.TwoCrown] = 1350,
                [GuardianRank.ThreeCrown] = 1500,
                [GuardianRank.FourCrown] = 1650,
                [GuardianRank.FiveCrown] = 1850,
            },
            [GuardianEvolution.Starlight] = new()
            {
                [GuardianRank.OneStar] = 700,
                [GuardianRank.TwoStar] = 900,
                [GuardianRank.ThreeStar] = 1050,
                [GuardianRank.FourStar] = 1200,
                [GuardianRank.FiveStar] = 1400,
                [GuardianRank.OneCrown] = 1250,
                [GuardianRank.TwoCrown] = 1400,
                [GuardianRank.ThreeCrown] = 1550,
                [GuardianRank.FourCrown] = 1700,
                [GuardianRank.FiveCrown] = 1900,
            },
            [GuardianEvolution.StarlightPlus] = new()
            {
                [GuardianRank.OneStar] = 750,
                [GuardianRank.TwoStar] = 950,
                [GuardianRank.ThreeStar] = 1100,
                [GuardianRank.FourStar] = 1250,
                [GuardianRank.FiveStar] = 1450,
                [GuardianRank.OneCrown] = 1300,
                [GuardianRank.TwoCrown] = 1450,
                [GuardianRank.ThreeCrown] = 1600,
                [GuardianRank.FourCrown] = 1750,
                [GuardianRank.FiveCrown] = 1950,
            },
        };

    // ── Core EXP formula ──────────────────────────────────────────────────────

    public static int ExpForLevel(GuardianEvolution evolution, GuardianRank rank, int level)
    {
        if (level is < 1 or > 9)
            throw new ArgumentOutOfRangeException(nameof(level), LanguageResource.LevelMustBeOneToNine);

        int rankIdx = (int)rank;

        return evolution == GuardianEvolution.Bronze && rank.IsStar()
            ? BronzeStarExpTable[rankIdx][level - 1]
            : CategoryBases[evolution] + (rankIdx * 100) + ((level - 1) * 10);
    }

    // ── Cumulative EXP ────────────────────────────────────────────────────────

    public static long TotalExpToPosition(GuardianPosition pos, int level)
    {
        long total = 0;
        GuardianEvolution[] allEvos = Enum.GetValues<GuardianEvolution>();
        bool targetCrown = pos.Rank.IsCrown();

        // Star phase — always computed
        GuardianEvolution starEvoLimit = targetCrown ? allEvos[^1] : pos.Evolution;
        GuardianRank starRankLimit = targetCrown ? GuardianRank.FiveStar : pos.Rank;
        int starLvlLimit = targetCrown ? 10 : level;

        foreach (GuardianEvolution evo in allEvos)
        {
            if (evo > starEvoLimit) break;
            bool lastEvo = evo == starEvoLimit;

            foreach (GuardianRank rank in Enum.GetValues<GuardianRank>().Where(r => r.IsStar()))
            {
                bool targetRank = lastEvo && rank == starRankLimit;
                int maxLvl = targetRank ? starLvlLimit : 10;

                for (int lvl = 1; lvl < maxLvl; lvl++)
                    total += ExpForLevel(evo, rank, lvl);

                if (targetRank) break;
            }

            if (lastEvo) break;
        }

        // Crown phase — only when target is a crown
        if (targetCrown)
        {
            foreach (GuardianEvolution evo in allEvos)
            {
                if (evo > pos.Evolution) break;
                bool lastEvo = evo == pos.Evolution;

                foreach (GuardianRank rank in Enum.GetValues<GuardianRank>().Where(r => r.IsCrown()))
                {
                    bool targetRank = lastEvo && rank == pos.Rank;
                    int maxLvl = targetRank ? level : 10;

                    for (int lvl = 1; lvl < maxLvl; lvl++)
                        total += ExpForLevel(evo, rank, lvl);

                    if (targetRank) break;
                }

                if (lastEvo) break;
            }
        }

        return total;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public sealed record EvolutionStep(
        string From,
        string To,
        GuardianEvolution Evolution,
        int Cost);

    public sealed record CalcResult(
        long ExpNeeded,
        int StrangeDustForExp,
        int StrangeDustForEvolutions,
        int TotalStrangeDust,
        IReadOnlyList<EvolutionStep> Evolutions,
        string? Error = null);

    public static string? Validate(
        GuardianPosition current, int currentLevel, int currentExp,
        GuardianPosition target, int targetLevel)
    {
        if (currentLevel is < 1 or > 10)
            return LanguageResource.StrangeDustCurrentLevelError;
        if (targetLevel is < 1 or > 10)
            return LanguageResource.StrangeDustTargetLevelError;
        if (currentExp < 0)
            return LanguageResource.StrangeDustErrorNegativeEXP;
        // Cannot go backwards — star → crown is always forward, never reverse
        return current.Rank.IsCrown() && target.Rank.IsStar()
            ? LanguageResource.StrangeDustCrownToStarError
            : target.Ordinal < current.Ordinal
            ? LanguageResource.StrangeDustCurrentHigherThanTargetError
            : target.Ordinal == current.Ordinal && targetLevel < currentLevel
            ? LanguageResource.StrangeDustCurrentLevelHigherThanTargetSameRank
            : target.Ordinal == current.Ordinal && targetLevel == currentLevel ? LanguageResource.StrangeDustAlreadyAtLevelError : null;
    }

    public static CalcResult Calculate(
        GuardianPosition current, int currentLevel, int currentExp,
        GuardianPosition target, int targetLevel)
    {
        string? error = Validate(current, currentLevel, currentExp, target, targetLevel);
        if (error is not null)
            return new CalcResult(0, 0, 0, 0, [], error);

        long expToCurrent = TotalExpToPosition(current, currentLevel);
        long expToTarget = TotalExpToPosition(target, targetLevel);
        long rawExp = expToTarget - expToCurrent - currentExp;
        long expNeeded = Math.Max(0, rawExp);

        int dustStacks = (int)Math.Ceiling((double)expNeeded / StrangeDustExpPerItem);
        int dustForExp = dustStacks * StrangeDustPerStack;

        IReadOnlyList<EvolutionStep> evolutions = CalculateEvolutions(current, target);
        int dustForEvo = evolutions.Sum(e => e.Cost);
        int total = dustForExp + dustForEvo;

        return new CalcResult(expNeeded, dustForExp, dustForEvo, total, evolutions);
    }

    // ── Evolution path ────────────────────────────────────────────────────────

    public static IReadOnlyList<EvolutionStep> CalculateEvolutions(
        GuardianPosition current, GuardianPosition target)
    {
        if (current.Ordinal >= target.Ordinal) return [];

        List<EvolutionStep> steps = [];
        IReadOnlyList<GuardianPosition> allPositions = GuardianPosition.AllPositions;

        int startIdx = -1;
        int endIdx = -1;

        for (int i = 0; i < allPositions.Count; i++)
        {
            GuardianPosition p = allPositions[i];

            if (startIdx == -1 &&
                p.Evolution == current.Evolution &&
                p.Rank == current.Rank)
                startIdx = i;

            if (endIdx == -1 &&
                p.Evolution == target.Evolution &&
                p.Rank == target.Rank)
                endIdx = i;

            if (startIdx != -1 && endIdx != -1)
                break;
        }

        if (startIdx < 0 || endIdx < 0 || startIdx >= endIdx) return [];

        for (int i = startIdx; i < endIdx; i++)
        {
            GuardianPosition from = allPositions[i];
            GuardianPosition to = allPositions[i + 1];

            // Only add evolution steps where rank advances (not level-ups)
            // The ordinal jump between adjacent positions in AllPositions is always 1
            // Cross-evolution jumps (FiveStar Bronze → OneStar Silver) are included
            // The special bridge from StarlightPlus FiveStar → Bronze OneCrown is also included
            steps.Add(new EvolutionStep(
                From: from.Label,
                To: to.Label,
                Evolution: from.Evolution,
                Cost: EvolutionCosts[from.Evolution][from.Rank]));
        }

        return steps;
    }
}