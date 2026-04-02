using BreakEternity;
using InfatalsFirestoneTools.Models;

namespace InfatalsFirestoneTools.Services.Optimizer;

public sealed class Optimizer
{
    // ── Game constants (private) ──────────────────────────────────────────────

    private const int FormationSize = 5;
    private const int MaxMissions = 90;
    private const int ReoptimizeEvery = 5;   // missions between full re-optimizations
    private const int MonteCarloRuns = 10000;

    private readonly CampaignDifficulty[] DifficultyOrder =
        Enum.GetValues<CampaignDifficulty>();

    // Hero scoring weights per role per mode
    // [campaign/arena][tank/dps] → (damage weight, health weight, armor weight)
    private (double Dmg, double Hp, double Arm) GetHeroWeights(bool arena, bool tank)
    {
        HeroWeights w = _input.HeroWeights;
        return (arena, tank) switch
        {
            (false, true) => (w.CampaignTankDamage, w.CampaignTankHealth, w.CampaignTankArmor),
            (false, false) => (w.CampaignDpsDamage, w.CampaignDpsHealth, w.CampaignDpsArmor),
            (true, true) => (w.ArenaTankDamage, w.ArenaTankHealth, w.ArenaTankArmor),
            (true, false) => (w.ArenaDpsDamage, w.ArenaDpsHealth, w.ArenaDpsArmor),
        };
    }

    // Crew slot thresholds
    private static readonly (int MinLevel, int Slots)[] CrewSlots =
        [(60, 6), (30, 5), (0, 4)];

    // ── Instance state ────────────────────────────────────────────────────────

    private readonly BattleEngine _battle = new();
    private readonly OptimizerInput _input;
    private readonly int _maxSlots;

    public Optimizer(OptimizerInput input)
    {
        _input = input;
        _maxSlots = CrewSlots.First(t => input.EngineerLevel >= t.MinLevel).Slots;
    }

    // ── Entry points ──────────────────────────────────────────────────────────

    public CampaignResult OptimizeCampaign()
    {
        if (_input.Machines.Count == 0)
            return new CampaignResult([], BigDouble.dZero, BigDouble.dZero, 0,
                DifficultyOrder.ToDictionary(k => k, _ => 0));

        ComputedMachine[]? best = null;
        int lastOptimized = 0;
        int totalStars = 0;
        Dictionary<CampaignDifficulty, int> lastCleared = DifficultyOrder.ToDictionary(k => k, _ => 0);
        ComputedMachine[] lastWinners = [];

        for (int mission = 1; mission <= MaxMissions; mission++)
        {
            if (best is null || mission - lastOptimized >= ReoptimizeEvery)
            {
                best = AssignCrew(SelectBestFive(arena: false), arena: false);
                lastOptimized = mission;
            }

            bool anyCleared = false;

            foreach (CampaignDifficulty diff in DifficultyOrder)
            {
                ComputedMachine[] arranged = ArrangeByRole(best, mission, diff);
                MachineStats[] enemyTeam = BuildEnemyTeam(mission, diff);

                if (Calculator.SquadPower(arranged, arena: false).lessThan(Calculator.RequiredPower(mission, diff)))
                    break;

                if (_battle.Run(arranged, enemyTeam).PlayerWon)
                {
                    totalStars++;
                    anyCleared = true;
                    lastCleared[diff] = mission;
                    lastWinners = [.. arranged];
                }
                else break;
            }

            if (!anyCleared && mission > 1) break;
        }

        (int extraStars, Dictionary<CampaignDifficulty, int>? finalCleared) = MonteCarloStars(lastWinners, lastCleared);
        totalStars += extraStars;

        return new CampaignResult(
            lastWinners,
            Calculator.SquadPower(lastWinners, arena: false),
            Calculator.SquadPower(lastWinners, arena: true),
            totalStars,
            finalCleared);
    }

    public ArenaResult OptimizeArena()
    {
        if (_input.Machines.Count == 0)
            return new ArenaResult([], BigDouble.dZero, BigDouble.dZero);

        ComputedMachine[] formation = ArrangeByRole(
            AssignCrew(SelectBestFive(arena: true), arena: true),
            mission: 1, difficulty: CampaignDifficulty.Easy);

        return new ArenaResult(
            formation,
            Calculator.SquadPower(formation, arena: true),
            Calculator.SquadPower(formation, arena: false));
    }

    // ── Formation selection ─────────────

    private ComputedMachine[] SelectBestFive(bool arena)
    {
        return _input.Machines
            .Select(m =>
            {
                ComputeStats(m, []);
                return (Machine: m, Power: Calculator.MachinePower(arena ? m.ArenaStats : m.BattleStats));
            })
            .OrderByDescending(x => x.Power.toDouble())
            .Take(FormationSize)
            .Select(x => x.Machine)
            .ToArray();
    }

    // ── Role arrangement ─────────────────

    private ComputedMachine[] ArrangeByRole(ComputedMachine[] team, int mission, CampaignDifficulty difficulty)
    {
        if (team.Length == 0) return [];

        MachineStats enemy = Calculator.EnemyStats(mission, difficulty);
        List<ComputedMachine> tanks = [];
        List<ComputedMachine> dps = [];
        List<ComputedMachine> useless = [];

        foreach (ComputedMachine m in team)
        {
            if (IsTank(m))
            {
                // Tanks that take more than 40% of their HP per hit are "useless" in front
                BigDouble dmgIn = Calculator.DamageTaken(enemy.Damage, m.BattleStats.Armor);
                if (dmgIn > m.BattleStats.Health * 0.4) useless.Add(m);
                else tanks.Add(m);
            }
            else
            {
                // DPS/healers that can't pierce enemy armor are "useless"
                BigDouble dmgOut = Calculator.DamageTaken(m.BattleStats.Damage, enemy.Armor);
                if (dmgOut == BigDouble.dZero) useless.Add(m);
                else dps.Add(m);
            }
        }

        // Useless units go front (most HP first) — they absorb hits before dying
        useless.Sort((a, b) => b.BattleStats.Health.CompareTo(a.BattleStats.Health));

        // Goliath always anchors the center tank slot
        ComputedMachine? goliath = tanks.FirstOrDefault(m => m.Name == "Goliath");
        List<ComputedMachine> otherTanks = tanks.Where(m => m.Name != "Goliath").ToList();

        // Tanks that can pierce enemy armor go after those that can't
        List<ComputedMachine> canHit = otherTanks
            .Where(m => Calculator.DamageTaken(m.BattleStats.Damage, enemy.Armor) > BigDouble.dZero)
            .OrderByDescending(m => m.BattleStats.Health.toDouble())
            .ToList();
        List<ComputedMachine> cannotHit = otherTanks
            .Where(m => Calculator.DamageTaken(m.BattleStats.Damage, enemy.Armor) == BigDouble.dZero)
            .OrderByDescending(m => m.BattleStats.Health.toDouble())
            .ToList();

        // Sort DPS ascending by damage — strongest is pulled out to the protected slot (second-to-last)
        dps.Sort((a, b) => a.BattleStats.Damage.CompareTo(b.BattleStats.Damage));

        ComputedMachine? protectedDps = null;
        if (dps.Count > 0 && team.Length == FormationSize)
        {
            protectedDps = dps[^1];
            dps.RemoveAt(dps.Count - 1);
        }

        // Build: [useless | cannot-hit tanks | Goliath | can-hit tanks | weaker DPS | (last slot)]
        // Then splice protectedDps in before the last element → slot index 3 in a 5-slot formation
        List<ComputedMachine> formation = [.. useless, .. cannotHit];
        if (goliath is not null) formation.Add(goliath);
        formation.AddRange(canHit);
        formation.AddRange(dps);

        if (protectedDps is not null && formation.Count > 0)
            formation.Insert(formation.Count - 1, protectedDps);
        else if (protectedDps is not null)
            formation.Add(protectedDps);

        return [.. formation];
    }

    // ── Crew assignment — Kuhn-Munkres (Hungarian) ───────────────────────────

    private ComputedMachine[] AssignCrew(ComputedMachine[] machines, bool arena)
    {
        ComputedHero[] ownedHeroes = _input.Heroes
            .Where(h => h.DamagePercentage > 0 || h.HealthPercentage > 0 || h.ArmorPercentage > 0)
            .OrderByDescending(h => h.DamagePercentage + h.HealthPercentage)
            .ToArray();

        if (ownedHeroes.Length == 0 || machines.Length == 0)
        {
            foreach (ComputedMachine m in machines) ComputeStats(m, []);
            return machines;
        }

        // Compute baseline stats (no crew) needed for scoring
        foreach (ComputedMachine m in machines) ComputeStats(m, []);

        // Expand each machine into _maxSlots individual slots for the assignment matrix
        ComputedMachine[] slots = machines
            .SelectMany(m => Enumerable.Repeat(m, _maxSlots))
            .ToArray();

        int n = ownedHeroes.Length;
        int m_ = slots.Length;
        int size = Math.Max(n, m_);

        // Weight matrix (1-indexed; row = hero, col = slot)
        BigDouble[,] weight = new BigDouble[size + 1, size + 1];
        for (int i = 1; i <= n; i++)
            for (int j = 1; j <= m_; j++)
                weight[i, j] = HeroScore(ownedHeroes[i - 1], slots[j - 1], arena);

        int[] assignment = Hungarian(weight, size);

        // Build crew map: machineId → [heroes]
        Dictionary<int, List<ComputedHero>> crewMap = [];
        for (int j = 1; j <= m_; j++)
        {
            int heroIdx = assignment[j] - 1;
            if (heroIdx < 0 || heroIdx >= n) continue;
            int id = slots[j - 1].Id;
            _ = crewMap.TryAdd(id, []);
            crewMap[id].Add(ownedHeroes[heroIdx]);
        }

        foreach (ComputedMachine machine in machines)
        {
            List<ComputedHero> crew = crewMap.GetValueOrDefault(machine.Id, []);
            ComputeStats(machine, crew);
            machine.Crew = crew;
        }

        return machines;
    }

    /// <summary>
    /// Standard Kuhn-Munkres maximum-weight matching.
    /// Returns matchY[slot] = heroIndex (1-indexed). 0 means unmatched.
    /// </summary>
    private static int[] Hungarian(BigDouble[,] weight, int size)
    {
        BigDouble[] lx = new BigDouble[size + 1];
        BigDouble[] ly = new BigDouble[size + 1];
        int[] matchY = new int[size + 1];
        BigDouble[] slack = new BigDouble[size + 1];
        int[] pre = new int[size + 1];
        bool[] visY = new bool[size + 1];
        BigDouble inf = new(1e300);
        BigDouble eps = new(1e-12);

        for (int i = 1; i <= size; i++)
            for (int j = 1; j <= size; j++)
                if (weight[i, j] > lx[i]) lx[i] = weight[i, j];

        for (int i = 1; i <= size; i++)
        {
            Array.Fill(visY, false);
            for (int j = 0; j <= size; j++) { slack[j] = inf; pre[j] = 0; }

            int curY = 0;
            matchY[0] = i;

            do
            {
                visY[curY] = true;
                int curX = matchY[curY];
                BigDouble delta = inf;
                int nextY = 0;

                for (int y = 1; y <= size; y++)
                {
                    if (visY[y]) continue;
                    BigDouble cur = lx[curX] + ly[y] - weight[curX, y];
                    if (cur < slack[y]) { slack[y] = cur; pre[y] = curY; }
                    if (slack[y] < delta) { delta = slack[y]; nextY = y; }
                }

                if (delta < eps) delta = BigDouble.dZero;
                if (delta > BigDouble.dZero)
                    for (int j = 0; j <= size; j++)
                    {
                        if (visY[j]) { lx[matchY[j]] -= delta; ly[j] += delta; }
                        else slack[j] -= delta;
                    }

                curY = nextY;
            }
            while (matchY[curY] != 0);

            while (curY != 0) { int prev = pre[curY]; matchY[curY] = matchY[prev]; curY = prev; }
        }

        return matchY;
    }

    // ── Hero scoring ──────────────

    private BigDouble HeroScore(ComputedHero hero, ComputedMachine machine, bool arena)
    {
        (double wDmg, double wHp, double wArm) = GetHeroWeights(arena, IsTank(machine));

        BigDouble baseScore =
            (new BigDouble(hero.DamagePercentage / 100.0) * wDmg) +
            (new BigDouble(hero.HealthPercentage / 100.0) * wHp) +
            (new BigDouble(hero.ArmorPercentage / 100.0) * wArm);

        if (baseScore <= BigDouble.dZero) return BigDouble.dZero;

        MachineStats stats = arena ? machine.ArenaStats : machine.BattleStats;
        BigDouble power = Calculator.MachinePower(stats);
        BigDouble logPower = power > BigDouble.dZero ? BigDouble.log10(power) + 1 : BigDouble.dOne;

        // Campaign score is squared for stronger differentiation between machines
        return arena ? baseScore * logPower : BigDouble.pow(baseScore * logPower, 2);
    }

    // ── Monte Carlo ───────────

    private (int ExtraStars, Dictionary<CampaignDifficulty, int> LastCleared) MonteCarloStars(ComputedMachine[] formation, Dictionary<CampaignDifficulty, int> lastCleared)
    {
        if (formation.Length == 0) return (0, lastCleared);

        int extra = 0;
        Dictionary<CampaignDifficulty, int> updated = new(lastCleared);
        BigDouble power = Calculator.SquadPower(formation, arena: false);

        foreach (CampaignDifficulty diff in DifficultyOrder)
        {
            int last = updated[diff];
            for (int mission = last + 1; mission <= MaxMissions; mission++)
            {
                if (power < Calculator.RequiredPower(mission, diff)) break;

                ComputedMachine[] arranged = ArrangeByRole(formation, mission, diff);
                MachineStats[] enemyTeam = BuildEnemyTeam(mission, diff);

                bool won = false;
                for (int i = 0; i < MonteCarloRuns; i++)
                {
                    if (_battle.Run(arranged, enemyTeam).PlayerWon)
                    {
                        won = true;
                        break;
                    }
                }

                if (won) { extra++; updated[diff] = mission; }
                else break;
            }
        }

        return (extra, updated);
    }

    // ── Stat computation helpers ──────────────────────────────────────────────

    private void ComputeStats(ComputedMachine machine, IReadOnlyList<ComputedHero> crew)
    {
        (BigDouble dmg, BigDouble hp, BigDouble arm) = Calculator.CalculateBattleAttributes(
            machine,
            crew,
            _input.GlobalRarityLevels,
            _input.Artifacts,
            _input.EngineerLevel);

        machine.BattleStats = new MachineStats { Damage = dmg, Health = hp, Armor = arm };

        machine.ArenaStats = Calculator.CalculateArenaAttributes(
            machine,
            machine.BattleStats,
            _input.GlobalRarityLevels,
            _input.ScarabLevel,
            _input.RiftRank);
    }

    private static MachineStats[] BuildEnemyTeam(int mission, CampaignDifficulty difficulty)
    {
        MachineStats stats = Calculator.EnemyStats(mission, difficulty);
        MachineStats[] team = new MachineStats[FormationSize];
        Array.Fill(team, stats);
        return team;
    }

    private static bool IsTank(ComputedMachine m)
    {
        return m.Specialization == MachineSpecialization.Tank;
    }
}

// ── Input / result types ──────────────────────────────────────────────────────

public sealed class OptimizerInput
{
    public required List<ComputedMachine> Machines { get; init; }
    public required IReadOnlyList<ComputedHero> Heroes { get; init; }
    public required IReadOnlyList<Artifact> Artifacts { get; init; }
    public required int EngineerLevel { get; init; }
    public required int ScarabLevel { get; init; }
    public required ChaosRiftRank RiftRank { get; init; }
    public required int GlobalRarityLevels { get; init; }
    public required HeroWeights HeroWeights { get; init; }
}

public sealed record CampaignResult(
    IReadOnlyList<ComputedMachine> Formation,
    BigDouble BattlePower,
    BigDouble ArenaPower,
    int TotalStars,
    Dictionary<CampaignDifficulty, int> LastCleared);

public sealed record ArenaResult(
    IReadOnlyList<ComputedMachine> Formation,
    BigDouble ArenaPower,
    BigDouble BattlePower);