namespace InfatalsFirestoneTools.Models
{
    public sealed record HeroStatic
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public HeroType Type { get; init; }
        public HeroClass Class { get; init; }
        public HeroAttackStyle AttackStyle { get; init; }
        public HeroSpecialization Specialization { get; init; }
        public HeroResource Resource { get; init; }
        public string Image { get; init; } = string.Empty;
        public int BaseDamage { get; init; }
        public int BaseHealth { get; init; }
        public int BaseArmor { get; init; }
        public double AttackSpeed { get; init; }
        public double CriticalChance { get; init; }
        public double CriticalDamage { get; init; }
        public double DodgeChance { get; init; }
    }
}
