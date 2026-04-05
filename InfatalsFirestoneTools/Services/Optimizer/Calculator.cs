using BreakEternity;
using InfatalsFirestoneTools.Models;

namespace InfatalsFirestoneTools.Services.Optimizer;

public static class Calculator
{
    // ── Game constants ─────────────────────────

    private static readonly BigDouble Base = new(1.05);

    private static readonly double[] RiftBonuses =
    {
        0.00, // Bronze
        0.00, // Silver
        0.00, // Gold
        0.00, // Pearl
        0.01, // Sapphire
        0.02, // Emerald
        0.03, // Ruby
        0.04, // Platinum
        0.05  // Diamond
    };

    // Power formula weights
    private const double PowerDmgWeight = 10.0;
    private const double PowerHpWeight = 1.0;
    private const double PowerArmWeight = 10.0;
    private const double PowerExponent = 0.7;

    // Enemy scaling 
    private const double MissionScaleFactor = 1.2;
    private const double MilestoneScaleFactor = 3.0;
    private const double PowerRequirementMilestoneFactor = 2.0;

    // Power-requirement easy-mode thresholds
    private const int EasyEarlyMaxMission = 10;
    private const int EasyMidMaxMission = 30;
    private const double EasyEarlyPct = 0.3;
    private const double EasyMidPct = 0.5;
    private const double DefaultPowerPct = 0.8;

    // Arena scaling
    private const double OverdriveBase = 0.25;
    private const double OverdrivePerRarity = 0.03;

    // Campaign difficulties 
    private static readonly BigDouble[] DifficultyMultipliers =
    {
        new(1),                  // Easy
        new(360),               // Normal
        new(2478600),           // Hard
        BigDouble.pow(10, 12) * 5.8,      // Insane
        BigDouble.pow(10, 18) * 2.92      // Nightmare
    };

    // Enemy stat cache — avoids recomputing the same mission/difficulty repeatedly
    private static readonly Dictionary<(int mission, CampaignDifficulty difficulty, double milestone), MachineStats> EnemyCache = [];
    // ── Public lookups ────────────────────────────────────────────────────────

    public static int GetGlobalRarityLevels(IReadOnlyList<Machine> machines)
    {
        int sum = 0;
        for (int i = 0; i < machines.Count; i++)
            sum += (int)machines[i].Rarity;
        return sum;
    }

    // ── Battle attributes ───

    public static (BigDouble Damage, BigDouble Health, BigDouble Armor) CalculateBattleAttributes(ComputedMachine machine, IReadOnlyList<ComputedHero> crew, int globalRarityLevels, IReadOnlyList<Artifact> artifacts, int engineerLevel)
    {
        BigDouble levelBonus = BigDouble.pow(Base, machine.Level - 1) - 1;
        BigDouble engineerBonus = BigDouble.pow(Base, engineerLevel - 1) - 1;
        BigDouble rarityBonus = BigDouble.pow(Base, (int)machine.Rarity + globalRarityLevels) - 1;
        BigDouble sacredBonus = BigDouble.pow(Base, machine.Dynamic.SacredLevel) - 1;
        BigDouble inscriptionBonus = BigDouble.pow(Base, machine.Dynamic.InscriptionLevel) - 1;

        BigDouble dmgBP = BigDouble.pow(Base, machine.Dynamic.DamageBlueprint) - 1;
        BigDouble hpBP = BigDouble.pow(Base, machine.Dynamic.HealthBlueprint) - 1;
        BigDouble armBP = BigDouble.pow(Base, machine.Dynamic.ArmorBlueprint) - 1;

        BigDouble artDmg = ArtifactBonus(artifacts, ArtifactStat.Damage);
        BigDouble artHp = ArtifactBonus(artifacts, ArtifactStat.Health);
        BigDouble artArm = ArtifactBonus(artifacts, ArtifactStat.Armor);

        (BigDouble crewDmg, BigDouble crewHp, BigDouble crewArm) = CrewBonus(crew);

        // Shared multipliers applied to every stat
        BigDouble CommonMultiplier(BigDouble bpBonus, BigDouble artBonus)
        {
            return (1 + levelBonus) * (1 + engineerBonus) * (1 + bpBonus)
            * (1 + rarityBonus) * (1 + sacredBonus) * (1 + inscriptionBonus)
            * (1 + artBonus);
        }

        BigDouble dmg = machine.Static.BaseDamage * CommonMultiplier(dmgBP, artDmg) * (1 + crewDmg);
        BigDouble hp = machine.Static.BaseHealth * CommonMultiplier(hpBP, artHp) * (1 + crewHp);
        BigDouble arm = machine.Static.BaseArmor * CommonMultiplier(armBP, artArm) * (1 + crewArm);

        return (dmg, hp, arm);
    }

    // ── Arena attributes  ──────

    public static MachineStats CalculateArenaAttributes(ComputedMachine machine, MachineStats battleStats, int globalRarityLevels, int scarabLevel, ChaosRiftRank riftRank)
    {
        // Scarab: floor((level - 3) / 2) + 1, clamped 0..1, multiplied by 0.002
        BigDouble scarabBonus = BigDouble.min(
            BigDouble.max(new BigDouble(Math.Floor((scarabLevel - 3.0) / 2.0) + 1), BigDouble.dZero) * 0.002,
            BigDouble.dOne);

        BigDouble riftBonus = new(RiftBonuses[(int)riftRank]);
        BigDouble mechFuryBonus = BigDouble.pow(Base, globalRarityLevels) - 1;
        BigDouble total = (1 + mechFuryBonus) * (1 + scarabBonus) * (1 + riftBonus);

        // Arena formula: base * log10(battle / base + 1)^2 * total
        BigDouble ArenaStat(BigDouble battle, int baseVal)
        {
            BigDouble b = new(baseVal);
            return b * BigDouble.pow(BigDouble.log10(battle / b) + 1, 2) * total;
        }

        BigDouble hp = ArenaStat(battleStats.Health, machine.Static.BaseHealth);
        return new MachineStats
        {
            Damage = ArenaStat(battleStats.Damage, machine.Static.BaseDamage),
            Health = hp,
            Armor = ArenaStat(battleStats.Armor, machine.Static.BaseArmor),
        };
    }

    // ── Overdrive chance  ───────────

    public static double CalculateOverdrive(ComputedMachine machine)
    {
        return OverdriveBase + ((int)machine.Rarity * OverdrivePerRarity);
    }

    // ── Damage taken  ──────────────

    public static BigDouble DamageTaken(BigDouble attackerDamage, BigDouble targetArmor)
    {
        BigDouble taken = attackerDamage - targetArmor;
        return taken > BigDouble.dZero ? taken : BigDouble.dZero;
    }

    // ── Power metric  ─────────────

    public static BigDouble MachinePower(MachineStats stats)
    {
        return BigDouble.pow(stats.Damage * PowerDmgWeight, PowerExponent)
        + BigDouble.pow(stats.Health * PowerHpWeight, PowerExponent)
        + BigDouble.pow(stats.Armor * PowerArmWeight, PowerExponent);
    }

    public static BigDouble SquadPower(IReadOnlyList<ComputedMachine> machines, bool arena)
    {
        BigDouble sum = BigDouble.dZero;

        for (int i = 0; i < machines.Count; i++)
        {
            ComputedMachine m = machines[i];
            sum += MachinePower(arena ? m.ArenaStats : m.BattleStats);
        }

        return sum;
    }

    // ── Enemy scaling ────────────────

    public static MachineStats EnemyStats(int mission, CampaignDifficulty difficulty, double milestoneBase = MilestoneScaleFactor)
    {
        (int mission, CampaignDifficulty difficulty, double milestoneBase) key = (mission, difficulty, milestoneBase);
        if (EnemyCache.TryGetValue(key, out MachineStats? cached))
            return cached;

        BigDouble diffMult = DifficultyMultipliers[(int)difficulty];
        int missionIdx = mission - 1;
        int milestoneCount = missionIdx / 10;

        BigDouble total = diffMult
            * BigDouble.pow(MissionScaleFactor, missionIdx)
            * BigDouble.pow(milestoneBase, milestoneCount);

        MachineStats stats = new()
        {
            Damage = new BigDouble(260) * total,
            Health = new BigDouble(1560) * total,
            Armor = new BigDouble(30) * total,
        };

        EnemyCache[key] = stats;
        return stats;
    }

    public static BigDouble RequiredPower(int mission, CampaignDifficulty difficulty)
    {
        BigDouble enemyPower = MachinePower(EnemyStats(mission, difficulty, PowerRequirementMilestoneFactor));

        double pct = difficulty == CampaignDifficulty.Easy
            ? mission <= EasyEarlyMaxMission ? EasyEarlyPct
              : mission <= EasyMidMaxMission ? EasyMidPct
              : DefaultPowerPct
            : DefaultPowerPct;

        return BigDouble.floor(enemyPower * 5 * pct / 100) * 100;
    }

    public static int GetMaxBlueprintLevel(int machineLevel)
    {
        return 5 + (machineLevel / 5 * 5);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static BigDouble ArtifactBonus(IReadOnlyList<Artifact> artifacts, ArtifactStat stat)
    {
        BigDouble product = BigDouble.dOne;
        foreach (Artifact a in artifacts)
        {
            if (a.Count <= 0 || a.Stat != stat)
                continue;
            product *= BigDouble.pow(1.0 + (a.Percentage / 100.0), a.Count);
        }
        return product - 1;
    }

    private static (BigDouble Dmg, BigDouble Hp, BigDouble Arm) CrewBonus(IReadOnlyList<ComputedHero> crew)
    {
        BigDouble dmg = 0, hp = 0, arm = 0;
        foreach (ComputedHero h in crew)
        {
            dmg += h.DamagePercentage / 100.0;
            hp += h.HealthPercentage / 100.0;
            arm += h.ArmorPercentage / 100.0;
        }
        return (dmg, hp, arm);
    }
}