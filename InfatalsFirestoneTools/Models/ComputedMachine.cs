namespace InfatalsFirestoneTools.Models
{
    public sealed class ComputedMachine
    {
        public MachineStatic Static { get; }
        public Machine Dynamic { get; }

        // Shortcuts to static data
        public int Id => Static.Id;
        public string Name => Static.Name;
        public string Image => Static.Image;
        public MachineSpecialization Specialization => Static.Specialization;
        public MachineTargetType TargetType => Static.TargetType;
        public Ability? Ability => Static.Ability;
        public MachineRarity Rarity => Dynamic.Rarity;
        public int Level => Dynamic.Level;


        // Assigned crew — populated by optimizer
        public List<ComputedHero> Crew { get; set; } = [];

        // Computed at runtime
        public MachineStats BattleStats { get; set; } = new();
        public MachineStats ArenaStats { get; set; } = new();

        public ComputedMachine(MachineStatic machineStatic, Machine machineDynamic)
        {
            Static = machineStatic;
            Dynamic = machineDynamic;
        }
    }
}
