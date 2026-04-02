namespace InfatalsFirestoneTools.Models
{
    public class Machine
    {
        // Dynamic Data
        public int Id { get; set; }
        public MachineRarity Rarity { get; set; } = MachineRarity.Common;
        public int Level { get; set; }
        public int DamageBlueprint { get; set; }
        public int HealthBlueprint { get; set; }
        public int ArmorBlueprint { get; set; }
        public int InscriptionLevel { get; set; }
        public int SacredLevel { get; set; }
    }
}
