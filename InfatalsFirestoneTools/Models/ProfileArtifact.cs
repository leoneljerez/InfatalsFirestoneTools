using InfatalsFirestoneTools.Services;
using Magic.IndexedDb;
using Magic.IndexedDb.SchemaAnnotations;

namespace InfatalsFirestoneTools.Models
{
    public class ProfileArtifact : MagicTableTool<ProfileArtifact>, IMagicTable<ProfileArtifact.DbSets>
    {
        public IMagicCompoundKey GetKeys()
        {
            return CreatePrimaryKey(x => x.Id, true);
        }

        public List<IMagicCompoundIndex>? GetCompoundIndexes()
        {
            return [];
        }

        public string GetTableName()
        {
            return "ProfileArtifact";
        }

        public IndexedDbSet GetDefaultDatabase()
        {
            return IndexedDbContext.Optimizer;
        }

        [MagicNotMapped]
        public DbSets Databases { get; } = new();

        public sealed class DbSets
        {
            public readonly IndexedDbSet Optimizer = IndexedDbContext.Optimizer;
        }

        [MagicName("id")]
        public int Id { get; set; }

        [MagicIndex]
        public int ProfileId { get; set; }
        public ArtifactStat Stat { get; set; }
        public int Percentage { get; set; }
        public int Count { get; set; }
    }
}
