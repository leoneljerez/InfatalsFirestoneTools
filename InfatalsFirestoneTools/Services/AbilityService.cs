using InfatalsFirestoneTools.Models;
using System.Collections.Frozen;

namespace InfatalsFirestoneTools.Services
{
    public class AbilityService
    {
        public FrozenDictionary<string, Ability> Abilities { get; } = new Dictionary<string, Ability>()
        {
            ["dmg_1x_150"] = new Ability { Effect = AbilityEffect.Damage, TargetType = AbilityTargetType.Enemy, TargetPosition = AbilityTargetPosition.Random, NumTargets = 1, ScaleStat = AbilityScaleStat.Damage, Multiplier = 2.0, Description = "dmg_1x_150_Description" },
            ["dmg_1x_160"] = new Ability { Effect = AbilityEffect.Damage, TargetType = AbilityTargetType.Enemy, TargetPosition = AbilityTargetPosition.Random, NumTargets = 1, ScaleStat = AbilityScaleStat.Damage, Multiplier = 1.6, Description = "dmg_1x_160_Description" },
            ["dmg_1x_200"] = new Ability { Effect = AbilityEffect.Damage, TargetType = AbilityTargetType.Enemy, TargetPosition = AbilityTargetPosition.Random, NumTargets = 1, ScaleStat = AbilityScaleStat.Damage, Multiplier = 2.0, Description = "dmg_1x_200_Description" },
            ["dmg_2x_60"] = new Ability { Effect = AbilityEffect.Damage, TargetType = AbilityTargetType.Enemy, TargetPosition = AbilityTargetPosition.Random, NumTargets = 2, ScaleStat = AbilityScaleStat.Damage, Multiplier = 0.6, Description = "dmg_2x_60_Description" },
            ["dmg_2x_130"] = new Ability { Effect = AbilityEffect.Damage, TargetType = AbilityTargetType.Enemy, TargetPosition = AbilityTargetPosition.Last, NumTargets = 2, ScaleStat = AbilityScaleStat.Damage, Multiplier = 1.3, Description = "dmg_2x_130_Description" },
            ["dmg_3x_120"] = new Ability { Effect = AbilityEffect.Damage, TargetType = AbilityTargetType.Enemy, TargetPosition = AbilityTargetPosition.Random, NumTargets = 3, ScaleStat = AbilityScaleStat.Damage, Multiplier = 1.2, Description = "dmg_3x_120_Description" },
            ["dmg_all_60"] = new Ability { Effect = AbilityEffect.Damage, TargetType = AbilityTargetType.Enemy, TargetPosition = AbilityTargetPosition.All, NumTargets = 5, ScaleStat = AbilityScaleStat.Damage, Multiplier = 0.6, Description = "dmg_all_60_Description" },
            ["dmg_all_80"] = new Ability { Effect = AbilityEffect.Damage, TargetType = AbilityTargetType.Enemy, TargetPosition = AbilityTargetPosition.All, NumTargets = 5, ScaleStat = AbilityScaleStat.Damage, Multiplier = 0.8, Description = "dmg_all_80_Description" },
            ["heal_lowest_1x_300"] = new Ability { Effect = AbilityEffect.Heal, TargetType = AbilityTargetType.Ally, TargetPosition = AbilityTargetPosition.Lowest, NumTargets = 1, ScaleStat = AbilityScaleStat.Damage, Multiplier = 3.0, Description = "heal_lowest_1x_300_Description" },
            ["heal_random_2x_350"] = new Ability { Effect = AbilityEffect.Heal, TargetType = AbilityTargetType.Ally, TargetPosition = AbilityTargetPosition.Random, NumTargets = 2, ScaleStat = AbilityScaleStat.Damage, Multiplier = 3.5, Description = "heal_random_2x_350_Description" },
            ["heal_all_150"] = new Ability { Effect = AbilityEffect.Heal, TargetType = AbilityTargetType.Ally, TargetPosition = AbilityTargetPosition.All, NumTargets = 5, ScaleStat = AbilityScaleStat.Damage, Multiplier = 1.5, Description = "heal_all_150_Description" },
            ["heal_self_hp_10"] = new Ability { Effect = AbilityEffect.Heal, TargetType = AbilityTargetType.Self, TargetPosition = AbilityTargetPosition.Self, NumTargets = 1, ScaleStat = AbilityScaleStat.Health, Multiplier = 0.1, Description = "heal_self_hp_10_Description" },
        }.ToFrozenDictionary();
    }
}
