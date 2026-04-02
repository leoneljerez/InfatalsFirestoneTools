using InfatalsFirestoneTools.Services;
using Magic.IndexedDb;
using Magic.IndexedDb.SchemaAnnotations;

namespace InfatalsFirestoneTools.Models
{
    public class ProfileMachine : MagicTableTool<ProfileMachine>, IMagicTable<ProfileMachine.DbSets>
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
            return "ProfileMachine";
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

        public int MachineId { get; set; }
        public MachineRarity Rarity { get; set; } = MachineRarity.Common;
        public int Level { get; set; }
        public int DamageBlueprint { get; set; }
        public int HealthBlueprint { get; set; }
        public int ArmorBlueprint { get; set; }
        public int InscriptionLevel { get; set; }
        public int SacredLevel { get; set; }
    }
}
