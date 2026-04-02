namespace InfatalsFirestoneTools.Models
{
    public sealed record GuardianStatic
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Image { get; init; } = string.Empty;
        public string TargetType { get; init; } = string.Empty;
        public int BaseDamage { get; init; }
        public double BaseCriticalChance { get; init; }
        public double BaseCriticalDaamge { get; init; }
        public double MaxAttackSpeed { get; init; }
    }
}
