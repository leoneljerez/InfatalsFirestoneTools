namespace InfatalsFirestoneTools.Models
{
    public sealed record MachineStatic
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public MachineSpecialization Specialization { get; init; }
        public MachineTargetType TargetType { get; init; }
        public string Image { get; init; } = string.Empty;
        public string AbilityKey { get; init; } = string.Empty;
        public int BaseDamage { get; init; }
        public int BaseHealth { get; init; }
        public int BaseArmor { get; init; }
        public Ability? Ability { get; init; }
    }
}
