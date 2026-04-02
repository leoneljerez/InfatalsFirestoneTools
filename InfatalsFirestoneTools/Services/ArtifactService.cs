using InfatalsFirestoneTools.Models;

namespace InfatalsFirestoneTools.Services
{
    public sealed class ArtifactService()
    {
        public static readonly int[] Tiers = [30, 35, 40, 45, 50, 55, 60, 65];
        private static readonly ArtifactStat[] Stats = Enum.GetValues<ArtifactStat>();

        public List<Artifact> CreateArtifacts()
        {
            return Stats.SelectMany(stat =>
                Tiers.Select(tier => new Artifact
                {
                    Stat = stat,
                    Percentage = tier,
                    Count = 0
                })
            ).ToList();
        }

        public static IEnumerable<Artifact> GetByStat(List<Artifact> artifacts, ArtifactStat stat)
        {
            return artifacts.Where(a => a.Stat == stat);
        }

        public static void ResetAll(List<Artifact> artifacts)
        {
            foreach (Artifact artifact in artifacts)
            {
                artifact.Count = 0;
            }
        }
    }
}
