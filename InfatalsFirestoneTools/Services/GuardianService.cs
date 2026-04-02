using InfatalsFirestoneTools.Models;

namespace InfatalsFirestoneTools.Services
{
    public class GuardianService
    {
        public IReadOnlyList<GuardianStatic> Guardians { get; } =
        [

            new() { Id = 1, Name = "VermilionLabel", Image = "img/guardians/Vermilion_Evo1", TargetType= "SingleLabel", BaseDamage = 20, BaseCriticalChance = 0.1, BaseCriticalDaamge = 3.0, MaxAttackSpeed = 0.1 },
            new() { Id = 2, Name = "GraceLabel", Image = "img/guardians/Grace_Evo1", TargetType= "SingleLabel", BaseDamage = 20, BaseCriticalChance = 0.1, BaseCriticalDaamge = 3.0, MaxAttackSpeed = 0.1 },
            new() { Id = 3, Name = "AnkaaLabel", Image = "img/guardians/Ankaa_Evo1", TargetType= "AoELabel", BaseDamage = 20, BaseCriticalChance = 0.1, BaseCriticalDaamge = 3.0, MaxAttackSpeed = 0.933 },
            new() { Id = 4, Name = "AzharLabel", Image = "img/guardians/Azhar_Evo1", TargetType= "SingleLabel", BaseDamage = 20, BaseCriticalChance = 0.1, BaseCriticalDaamge = 3.0, MaxAttackSpeed = 0.1 },

        ];
    }
}
