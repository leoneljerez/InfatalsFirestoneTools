using InfatalsFirestoneTools.Models;

namespace InfatalsFirestoneTools.Services
{
    public class OptimizerDataFactory(MachineService machineService, HeroService heroService, ArtifactService artifactService)
    {
        public OptimizerData Create()
        {
            return new()
            {
                Machines = machineService.Machines.Select(m => new Machine { Id = m.Id }).ToList(),
                Heroes = heroService.Heroes.Select(h => new Hero { Id = h.Id }).ToList(),
                Artifacts = artifactService.CreateArtifacts(),
                HeroWeights = new HeroWeights(),
            };
        }
    }
}
