using InfatalsFirestoneTools.Models;

namespace InfatalsFirestoneTools.Services
{
    public class OptimizerDataFactory(MachineService machineService, HeroService heroService, ArtifactService artifactService)
    {
        public OptimizerData Create()
        {
            return new()
            {
                Machines = [.. machineService.MachinesRaw.Select(m => new Machine { Id = m.Id })],
                Heroes = [.. heroService.HeroesRaw.Select(h => new Hero { Id = h.Id })],
                Artifacts = artifactService.CreateArtifacts(),
                HeroWeights = new HeroWeights(),
            };
        }
    }
}
