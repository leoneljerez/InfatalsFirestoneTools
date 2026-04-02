namespace InfatalsFirestoneTools.Models
{
    public class Ability
    {
        public string Description { get; init; } = string.Empty;
        public AbilityEffect Effect { get; init; }
        public AbilityTargetType TargetType { get; init; }
        public AbilityTargetPosition TargetPosition { get; init; }
        public int NumTargets { get; init; }
        public AbilityScaleStat ScaleStat { get; init; }
        public double Multiplier { get; init; }
    }
}
