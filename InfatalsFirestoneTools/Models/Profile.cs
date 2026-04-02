using InfatalsFirestoneTools.Services;
using Magic.IndexedDb;
using Magic.IndexedDb.SchemaAnnotations;

namespace InfatalsFirestoneTools.Models
{
    public class Profile : MagicTableTool<Profile>, IMagicTable<Profile.DbSets>
    {
        public IMagicCompoundKey GetKeys()
        {
            return CreatePrimaryKey(x => x.Id, true);
        }

        public string GetTableName()
        {
            return "Profile";
        }

        public IndexedDbSet GetDefaultDatabase()
        {
            return IndexedDbContext.Optimizer;
        }

        public List<IMagicCompoundIndex>? GetCompoundIndexes()
        {
            return [];
        }

        [MagicNotMapped]
        public DbSets Databases { get; } = new();

        public sealed class DbSets
        {
            public readonly IndexedDbSet Optimizer = IndexedDbContext.Optimizer;
        }

        [MagicName("id")]
        public int Id { get; set; }
        public string Name { get; set; } = "Default";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int EngineerLevel { get; set; }
        public int ScarabLevel { get; set; }
        public ChaosRiftRank ChaosRiftRank { get; set; } = ChaosRiftRank.Bronze;
        public OptimizeMode OptimizeMode { get; set; } = OptimizeMode.Campaign;
        public HeroWeights HeroWeights { get; set; } = new();
    }
}
