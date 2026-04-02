using InfatalsFirestoneTools.Resources;

namespace InfatalsFirestoneTools.Models
{
    public sealed record MachineRank(MachineLevelRankType Type, MachineLevelRankTier Tier, int Count, string DisplayText)
    {
        /// <summary>
        /// Derives rank from a machine level (1–150+).
        /// Returns null for level 0 (unranked).
        /// </summary>
        public static MachineRank? FromLevel(int level)
        {
            if (level < 1) return null;

            MachineLevelRankType type;
            int adjustedLevel;

            if (level <= 50)
            {
                type = MachineLevelRankType.Star;
                adjustedLevel = level;
            }
            else if (level <= 100)
            {
                type = MachineLevelRankType.Crown;
                adjustedLevel = level - 50;
            }
            else if (level <= 150)
            {
                type = MachineLevelRankType.Wings;
                adjustedLevel = level - 100;
            }
            else
            {
                return new MachineRank(MachineLevelRankType.Wings, MachineLevelRankTier.StarlightPlus, 5, LanguageResource.StarlightPlusWingsMachineMaxRank);
            }

            int count = ((adjustedLevel - 1) % 5) + 1;
            int tierIndex = (adjustedLevel - 1) / 5;
            MachineLevelRankTier tier = (MachineLevelRankTier)Math.Min(tierIndex, (int)MachineLevelRankTier.StarlightPlus);
            string label = $"{count} {tier} {type}{(count > 1 ? "s" : "")}";

            return new MachineRank(type, tier, count, label);
        }

        /// <summary>
        /// Returns the image base path (without extension) for this rank icon.
        /// </summary>
        public string ImageBasePath => $"img/ui/ranks/{FileName}";

        private string FileName => (Type, Tier) switch
        {
            (MachineLevelRankType.Star, MachineLevelRankTier.Bronze) => "star1Bronze",
            (MachineLevelRankType.Star, MachineLevelRankTier.Silver) => "star2Silver",
            (MachineLevelRankType.Star, MachineLevelRankTier.Gold) => "star3Gold",
            (MachineLevelRankType.Star, MachineLevelRankTier.Platinum) => "star4Platinum",
            (MachineLevelRankType.Star, MachineLevelRankTier.Ruby) => "star5Ruby",
            (MachineLevelRankType.Star, MachineLevelRankTier.Sapphire) => "star6Sepphire",
            (MachineLevelRankType.Star, MachineLevelRankTier.Pearl) => "star7Pearl",
            (MachineLevelRankType.Star, MachineLevelRankTier.Diamond) => "star8Diamond",
            (MachineLevelRankType.Star, MachineLevelRankTier.Starlight) => "star9Starlight",
            (MachineLevelRankType.Star, MachineLevelRankTier.StarlightPlus) => "star10StarlightPlus",
            (MachineLevelRankType.Crown, MachineLevelRankTier.Bronze) => "crown11Bronze",
            (MachineLevelRankType.Crown, MachineLevelRankTier.Silver) => "crown12Silver",
            (MachineLevelRankType.Crown, MachineLevelRankTier.Gold) => "crown13Gold",
            (MachineLevelRankType.Crown, MachineLevelRankTier.Platinum) => "crown14Platinum",
            (MachineLevelRankType.Crown, MachineLevelRankTier.Ruby) => "crown15Ruby",
            (MachineLevelRankType.Crown, MachineLevelRankTier.Sapphire) => "crown16Sepphire",
            (MachineLevelRankType.Crown, MachineLevelRankTier.Pearl) => "crown17Pearl",
            (MachineLevelRankType.Crown, MachineLevelRankTier.Diamond) => "crown18Diamond",
            (MachineLevelRankType.Crown, MachineLevelRankTier.Starlight) => "crown19Starlight",
            (MachineLevelRankType.Crown, MachineLevelRankTier.StarlightPlus) => "crown20StarlightPlus",
            (MachineLevelRankType.Wings, MachineLevelRankTier.Bronze) => "wings21Bronze",
            (MachineLevelRankType.Wings, MachineLevelRankTier.Silver) => "wings22Silver",
            (MachineLevelRankType.Wings, MachineLevelRankTier.Gold) => "wings23Gold",
            (MachineLevelRankType.Wings, MachineLevelRankTier.Platinum) => "wings24Platinum",
            (MachineLevelRankType.Wings, MachineLevelRankTier.Ruby) => "wings25Ruby",
            (MachineLevelRankType.Wings, MachineLevelRankTier.Sapphire) => "wings26Sapphire",
            (MachineLevelRankType.Wings, MachineLevelRankTier.Pearl) => "wings27Pearl",
            (MachineLevelRankType.Wings, MachineLevelRankTier.Diamond) => "wings28Diamond",
            (MachineLevelRankType.Wings, MachineLevelRankTier.Starlight) => "wings29Starlight",
            (MachineLevelRankType.Wings, MachineLevelRankTier.StarlightPlus) => "wings30StarligtPlus",
            _ => "star1Bronze",
        };
    }
}