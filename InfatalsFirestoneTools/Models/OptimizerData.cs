namespace InfatalsFirestoneTools.Models
{
    public class OptimizerData
    {
        public List<Machine> Machines { get; set; } = [];
        public List<Hero> Heroes { get; set; } = [];
        public List<Artifact> Artifacts { get; set; } = [];
        public int EngineerLevel { get; set; }
        public int ScarabLevel { get; set; }
        public ChaosRiftRank ChaosRiftRank { get; set; } = ChaosRiftRank.Bronze;
        public OptimizeMode OptimizeMode { get; set; } = OptimizeMode.Campaign;
        public HeroWeights HeroWeights { get; set; } = new();
    }
}
