namespace InfatalsFirestoneTools.Models
{
    public sealed class ComputedHero
    {
        public HeroStatic Static { get; }
        public Hero Dynamic { get; }

        // Shortcuts to static data
        public int Id => Static.Id;
        public string Name => Static.Name;
        public string Image => Static.Image;
        public HeroSpecialization Specialization => Static.Specialization;
        public int DamagePercentage => Dynamic.DamagePercentage;
        public int HealthPercentage => Dynamic.HealthPercentage;
        public int ArmorPercentage => Dynamic.ArmorPercentage;

        public ComputedHero(HeroStatic staticData, Hero dynamic)
        {
            Static = staticData;
            Dynamic = dynamic;
        }
    }
}
