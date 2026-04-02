using InfatalsFirestoneTools.Services;
using Magic.IndexedDb;
using Magic.IndexedDb.SchemaAnnotations;

namespace InfatalsFirestoneTools.Models
{
    public class ProfileHero : MagicTableTool<ProfileHero>, IMagicTable<ProfileHero.DbSets>
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
            return "ProfileHero";
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

        public int HeroId { get; set; }
        public int DamagePercentage { get; set; }
        public int HealthPercentage { get; set; }
        public int ArmorPercentage { get; set; }
    }
}
