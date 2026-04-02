namespace InfatalsFirestoneTools.Models
{
    /***************** Machines Enums *****************/
    public enum MachineSpecialization
    {
        Tank = 0,
        Damage = 1,
        Healer = 2
    }
    public enum MachineTargetType
    {
        Single = 0,
        Multi = 1
    }
    public enum MachineRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythic = 5,
        Titan = 6,
        Angel = 7,
        Celestial = 8
    }

    public enum MachineLevelRankTier
    {
        Bronze = 0,
        Silver = 1,
        Gold = 2,
        Platinum = 3,
        Ruby = 4,
        Sapphire = 5,
        Pearl = 6,
        Diamond = 7,
        Starlight = 8,
        StarlightPlus = 9
    }

    public enum MachineLevelRankType
    {
        Star = 0,
        Crown = 1,
        Wings = 2
    }

    /***************** Abilities Enums *****************/
    public enum AbilityTargetType
    {
        Self = 0,
        Ally = 1,
        Enemy = 2
    }
    public enum AbilityTargetPosition
    {
        Self = 0,
        All = 1,
        Lowest = 2,
        Last = 3,
        Random = 4
    }
    public enum AbilityEffect
    {
        Damage = 0,
        Heal = 1
    }
    public enum AbilityScaleStat
    {
        Damage = 0,
        Health = 1
    }

    /***************** Heroes Enums *****************/
    public enum HeroType
    {
        Hero = 0,
        Mercenary = 1,
        God = 2
    }
    public enum HeroClass
    {
        Warrior = 0,
        Rogue = 1,
        Mage = 2,
        Priest = 3,
        Paladin = 4,
        Druid = 5,
        Hunter = 6,
        Shaman = 7,
        Warlock = 8,
        Engineer = 9,
        Monk = 10,
        God = 11
    }
    public enum HeroAttackStyle
    {
        Melee = 0,
        Ranged = 1,
        Spellcaster = 2
    }
    public enum HeroSpecialization
    {
        Tank = 0,
        Damage = 1,
        Healer = 2
    }
    public enum HeroResource
    {
        Mana = 0,
        Energy = 1,
        Rage = 2
    }

    /***************** Artifacts Enums *****************/
    public enum ArtifactStat
    {
        Damage = 0,
        Health = 1,
        Armor = 2
    }

    /***************** Guardian Enums *****************/
    public enum GuardianEvolution
    {
        Bronze = 0,
        Silver = 1,
        Gold = 2,
        Platinum = 3,
        Ruby = 4,
        Sapphire = 5,
        Pearl = 6,
        Diamond = 7,
        Starlight = 8,
        StarlightPlus = 9
    }

    public enum GuardianRank
    {
        OneStar = 0,
        TwoStar = 1,
        ThreeStar = 2,
        FourStar = 3,
        FiveStar = 4,
        OneCrown = 5,
        TwoCrown = 6,
        ThreeCrown = 7,
        FourCrown = 8,
        FiveCrown = 9
    }

    /***************** Profile Enums *****************/
    public enum ChaosRiftRank
    {
        Bronze = 0,
        Silver = 1,
        Gold = 2,
        Pearl = 3,
        Sapphire = 4,
        Emerald = 5,
        Ruby = 6,
        Platinum = 7,
        Diamond = 8
    }
    public enum OptimizeMode
    {
        Campaign = 0,
        Arena = 1
    }

    /***************** UI Enums *****************/
    public enum SortOptionsMachine
    {
        Default = 0,
        NameAtoZ = 1,
        NameZtoA = 2,
        LevelHighToLow = 3,
        LevelLowToHigh = 4,
        RarityHighToLow = 5,
        RarityLowToHigh = 6,
        DamageBPHighToLow = 7,
        DamageBPLowToHigh = 8,
        HealthBPHighToLow = 9,
        HealthBPLowToHigh = 10,
        ArmorBPHighToLow = 11,
        ArmorBPLowToHigh = 12,
        ConfiguredFirst = 13,
        UnconfiguredFirst = 14,
    }

    public enum SortOptionsHero
    {
        Default = 0,
        NameAtoZ = 1,
        NameZtoA = 2,
        DamageHighToLow = 3,
        DamageLowToHigh = 4,
        HealthHighToLow = 5,
        HealthLowToHigh = 6,
        ArmorHighToLow = 7,
        ArmorLowToHigh = 8,
        ConfiguredFirst = 9,
        UnconfiguredFirst = 10
    }

    /***************** Optimizer Enums *****************/
    public enum CampaignDifficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Insane = 3,
        Nightmare = 4
    }
}