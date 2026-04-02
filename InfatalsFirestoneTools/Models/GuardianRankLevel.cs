namespace InfatalsFirestoneTools.Models;

using InfatalsFirestoneTools.Resources;

public static class GuardianRankExtensions
{
    public static bool IsCrown(this GuardianRank rank)
    {
        return (int)rank >= 5;
    }

    public static bool IsStar(this GuardianRank rank)
    {
        return (int)rank < 5;
    }

    public static int StarCount(this GuardianRank rank)
    {
        return rank.IsStar() ? (int)rank + 1 : 0;
    }

    public static int CrownCount(this GuardianRank rank)
    {
        return rank.IsCrown() ? (int)rank - 4 : 0;
    }

    public static string ToLabel(this GuardianRank rank)
    {
        return rank switch
        {
            GuardianRank.OneStar => $"1 {LanguageResource.Star}",
            GuardianRank.TwoStar => $"2 {LanguageResource.Stars}",
            GuardianRank.ThreeStar => $"3 {LanguageResource.Stars}",
            GuardianRank.FourStar => $"4 {LanguageResource.Stars}",
            GuardianRank.FiveStar => $"5 {LanguageResource.Stars}",
            GuardianRank.OneCrown => $"1 {LanguageResource.Crown}",
            GuardianRank.TwoCrown => $"2 {LanguageResource.Crowns}",
            GuardianRank.ThreeCrown => $"3 {LanguageResource.Crowns}",
            GuardianRank.FourCrown => $"4 {LanguageResource.Crowns}",
            GuardianRank.FiveCrown => $"5 {LanguageResource.Crowns}",
            _ => rank.ToString(),
        };
    }

    public static string ToLabel(this GuardianEvolution evolution)
    {
        return LanguageResource.ResourceManager.GetString(evolution.ToString()) ?? evolution.ToString();
    }

    /// <summary>
    /// Returns the base image path for a single rank icon at the given evolution tier.
    /// The filename encodes the icon graphic — callers repeat it N times for count.
    /// Mirrors RANK_FILE_MAP from config.js exactly, including the typos in the source.
    /// </summary>
    public static string RankIconPath(this GuardianRank rank, GuardianEvolution evolution)
    {
        string file = rank.IsStar()
            ? StarIconFile(evolution)
            : CrownIconFile(evolution);
        return $"img/ui/ranks/{file}";
    }

    // Star icon files — one file per tier, repeated by count in the UI
    private static string StarIconFile(GuardianEvolution evolution)
    {
        return evolution switch
        {
            GuardianEvolution.Bronze => "star1Bronze",
            GuardianEvolution.Silver => "star2Silver",
            GuardianEvolution.Gold => "star3Gold",
            GuardianEvolution.Platinum => "star4Platinum",
            GuardianEvolution.Ruby => "star5Ruby",
            GuardianEvolution.Sapphire => "star6Sepphire",      // typo matches game files
            GuardianEvolution.Pearl => "star7Pearl",
            GuardianEvolution.Diamond => "star8Diamond",
            GuardianEvolution.Starlight => "star9Starlight",
            GuardianEvolution.StarlightPlus => "star10StarlightPlus",
            _ => "star1Bronze",
        };
    }

    // Crown icon files — one file per tier, repeated by count in the UI
    private static string CrownIconFile(GuardianEvolution evolution)
    {
        return evolution switch
        {
            GuardianEvolution.Bronze => "crown11Bronze",
            GuardianEvolution.Silver => "crown12Silver",
            GuardianEvolution.Gold => "crown13Gold",
            GuardianEvolution.Platinum => "crown14Platinum",
            GuardianEvolution.Ruby => "crown15Ruby",
            GuardianEvolution.Sapphire => "crown16Sepphire",    // typo matches game files
            GuardianEvolution.Pearl => "crown17Pearl",
            GuardianEvolution.Diamond => "crown18Diamond",
            GuardianEvolution.Starlight => "crown19Starlight",
            GuardianEvolution.StarlightPlus => "crown20StarlightPlus",
            _ => "crown11Bronze",
        };
    }
}

/// <summary>
/// A combined position in the guardian rank system — one evolution + one rank.
/// Used as the single value type for the combined dropdown.
/// </summary>
public sealed record GuardianPosition(GuardianEvolution Evolution, GuardianRank Rank)
{
    public static IReadOnlyList<GuardianPosition> AllPositions { get; } = BuildAll();

    public string Label =>
        $"{Rank.ToLabel()} {Evolution.ToLabel()}";

    public string IconPath =>
        Rank.RankIconPath(Evolution);

    public int IconCount =>
        Rank.IsStar() ? Rank.StarCount() : Rank.CrownCount();

    /// <summary>
    /// Strict ordering: all stars (all evolutions) come before all crowns.
    /// Within stars: Bronze 1→5, Silver 1→5, ... StarlightPlus 1→5.
    /// Within crowns: same pattern.
    /// This matches the game's actual progression and prevents going backwards.
    /// </summary>
    public int Ordinal =>
        Rank.IsStar()
            ? ((int)Evolution * 5) + Rank.StarCount() - 1
            : 500 + ((int)Evolution * 5) + Rank.CrownCount() - 1;

    public bool IsAfter(GuardianPosition other)
    {
        return Ordinal > other.Ordinal;
    }

    public bool IsSameOrAfter(GuardianPosition other)
    {
        return Ordinal >= other.Ordinal;
    }

    private static List<GuardianPosition> BuildAll()
    {
        List<GuardianPosition> list = [];

        // Stars first — all evolutions × all star ranks in progression order
        foreach (GuardianEvolution evo in Enum.GetValues<GuardianEvolution>())
            foreach (GuardianRank rank in Enum.GetValues<GuardianRank>().Where(r => r.IsStar()))
                list.Add(new GuardianPosition(evo, rank));

        // Crowns second — same pattern
        foreach (GuardianEvolution evo in Enum.GetValues<GuardianEvolution>())
            foreach (GuardianRank rank in Enum.GetValues<GuardianRank>().Where(r => r.IsCrown()))
                list.Add(new GuardianPosition(evo, rank));

        return list;
    }
}