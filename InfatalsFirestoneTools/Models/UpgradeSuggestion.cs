namespace InfatalsFirestoneTools.Models
{
    public class UpgradeSuggestion
    {
        public string MachineName { get; set; } = string.Empty;
        public string MachineImage { get; set; } = string.Empty;
        public MachineSpecialization Specialization { get; set; }
        public int CurrentLevel { get; set; }
        public int SuggestedLevel { get; set; }
        public int CurrentDamageBlueprint { get; set; }
        public int SuggestedDamageBlueprint { get; set; }
        public int CurrentHealthBlueprint { get; set; }
        public int SuggestedHealthBlueprint { get; set; }
        public int CurrentArmorBlueprint { get; set; }
        public int SuggestedArmorBlueprint { get; set; }
    }

    public sealed record UpgradeSuggestionResult
    {
        public IReadOnlyList<UpgradeSuggestion> Suggestions { get; set; } = [];
        public int TargetMission { get; set; }
        public CampaignDifficulty TargetDifficulty { get; set; }
        public bool HasCorrectComposition { get; set; }
        public string? CompositionWarning { get; set; } = string.Empty;
    }
}
