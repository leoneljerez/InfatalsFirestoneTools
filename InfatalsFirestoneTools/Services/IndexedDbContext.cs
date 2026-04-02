using Magic.IndexedDb;
using Magic.IndexedDb.Interfaces;

namespace InfatalsFirestoneTools.Services
{
    public class IndexedDbContext : IMagicRepository
    {
        public static readonly IndexedDbSet Optimizer = new("Optimizer");
    }
}
