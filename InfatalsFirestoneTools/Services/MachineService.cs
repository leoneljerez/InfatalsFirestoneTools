using InfatalsFirestoneTools.Models;
using System.Collections.Frozen;

namespace InfatalsFirestoneTools.Services
{
    public class MachineService
    {
        public IReadOnlyList<MachineStatic> Machines { get; }

        public MachineService(AbilityService abilityService)
        {
            FrozenDictionary<string, Ability> ab = abilityService.Abilities;

            Machines =
            [
                new MachineStatic{ Id = 1, Name = "AegisLabel", Specialization = MachineSpecialization.Damage, TargetType = MachineTargetType.Single, Image = "img/machines/aegis164", AbilityKey = "dmg_1x_160", BaseDamage = 890, BaseHealth = 5100, BaseArmor = 115 },
                new MachineStatic{ Id = 2, Name = "CloudfistLabel", Specialization = MachineSpecialization.Damage, TargetType = MachineTargetType.Single, Image = "img/machines/baloon164", AbilityKey = "dmg_1x_200", BaseDamage = 880, BaseHealth = 6500, BaseArmor = 125 },
                new MachineStatic{ Id = 3, Name = "FirecrackerLabel", Specialization = MachineSpecialization.Damage, TargetType = MachineTargetType.Single, Image = "img/machines/fireCracker164", AbilityKey = "dmg_1x_150", BaseDamage = 910, BaseHealth = 4900, BaseArmor = 110 },
                new MachineStatic{ Id = 4, Name = "TalosLabel", Specialization = MachineSpecialization.Damage, TargetType = MachineTargetType.Single, Image = "img/machines/talos164", AbilityKey = "dmg_1x_200", BaseDamage = 860, BaseHealth = 6000, BaseArmor = 130 },
                new MachineStatic{ Id = 5, Name = "HarvesterLabel", Specialization = MachineSpecialization.Damage, TargetType = MachineTargetType.Multi, Image = "img/machines/webster164", AbilityKey = "dmg_2x_130", BaseDamage = 960, BaseHealth = 5500, BaseArmor = 125 },
                new MachineStatic{ Id = 6, Name = "JudgementLabel", Specialization = MachineSpecialization.Damage, TargetType = MachineTargetType.Multi, Image = "img/machines/judgement164", AbilityKey = "dmg_all_60", BaseDamage = 1080, BaseHealth = 4700, BaseArmor = 90 },
                new MachineStatic{ Id = 7, Name = "ThunderclapLabel", Specialization = MachineSpecialization.Damage, TargetType = MachineTargetType.Multi, Image = "img/machines/thunderclap164", AbilityKey = "dmg_3x_120", BaseDamage = 1050, BaseHealth = 5200, BaseArmor = 100 },
                new MachineStatic{ Id = 8, Name = "CuratorLabel", Specialization = MachineSpecialization.Healer, TargetType = MachineTargetType.Single, Image = "img/machines/curator164", AbilityKey = "heal_lowest_1x_300", BaseDamage = 380, BaseHealth = 4100, BaseArmor = 150 },
                new MachineStatic{ Id = 9, Name = "HunterLabel", Specialization = MachineSpecialization.Healer, TargetType = MachineTargetType.Multi, Image = "img/machines/hunter164", AbilityKey = "heal_random_2x_350", BaseDamage = 400, BaseHealth = 4900, BaseArmor = 130 },
                new MachineStatic{ Id = 10, Name = "SentinelLabel", Specialization = MachineSpecialization.Healer, TargetType = MachineTargetType.Multi, Image = "img/machines/sentinel164", AbilityKey = "heal_all_150", BaseDamage = 390, BaseHealth = 4400, BaseArmor = 170 },
                new MachineStatic{ Id = 11, Name = "EarthshattererLabel", Specialization = MachineSpecialization.Tank, TargetType = MachineTargetType.Multi, Image = "img/machines/earthshatter164", AbilityKey = "dmg_all_80", BaseDamage = 510, BaseHealth = 10500, BaseArmor = 270 },
                new MachineStatic{ Id = 12, Name = "FortressLabel", Specialization = MachineSpecialization.Tank, TargetType = MachineTargetType.Multi, Image = "img/machines/fortress164", AbilityKey = "dmg_2x_60", BaseDamage = 460, BaseHealth = 11000, BaseArmor = 300 },
                new MachineStatic{ Id = 13, Name = "GoliathLabel", Specialization = MachineSpecialization.Tank, TargetType = MachineTargetType.Single, Image = "img/machines/goliath164", AbilityKey = "heal_self_hp_10", BaseDamage = 430, BaseHealth = 12000, BaseArmor = 280 }
            ];

            // Attach abilities — create new records with Ability set
            Machines = Machines
                .Select(m => m with { Ability = ab.GetValueOrDefault(m.AbilityKey) })
                .ToList()
                .AsReadOnly();
        }
    }
}
