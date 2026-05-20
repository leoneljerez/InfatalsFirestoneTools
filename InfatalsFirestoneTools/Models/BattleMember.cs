namespace InfatalsFirestoneTools.Models
{
    public struct BattleMember
    {
        public double Hp;
        public double MaxHp;
        public double Dmg;
        public double Arm;
        public bool IsDead;

        // ── Flattened Ability Data ──
        public bool HasAbility;
        public double OverdriveChance;

        public AbilityEffect Effect;
        public AbilityTargetType TargetType;
        public AbilityTargetPosition TargetPosition;
        public int NumTargets;
        public AbilityScaleStat ScaleStat;
        public double MultiplierLog; // Pre-calculated Math.Log10(Multiplier)
    }
}
