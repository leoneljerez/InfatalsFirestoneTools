using BreakEternity;
using InfatalsFirestoneTools.Models;
using InfatalsFirestoneTools.Services.Optimizer;
using Microsoft.JSInterop;
using System.Text.Json;

namespace InfatalsFirestoneTools.Services;

/// <summary>
/// Runs the optimizer via the Rust/WASM engine when available,
/// falling back to the C# implementation automatically.
/// Register as scoped in Program.cs.
/// </summary>
public sealed class OptimizerService(
    IJSRuntime js,
    MachineService machineService,
    HeroService heroService)
{
    public OptimizationResult? LastResult { get; private set; }

    public async Task<OptimizationResult> RunAsync(OptimizerData data)
    {
        if (await IsWasmAvailableAsync())
        {
            try
            {
                LastResult = await RunWasmAsync(data);
                return LastResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WASM] execution failed, falling back to C#: {ex.Message}");
            }
        }

        LastResult = RunCSharp(data);
        return LastResult;
    }

    // ── WASM availability check ───────────────────────────────────────────────

    private async Task<bool> IsWasmAvailableAsync()
    {
        try
        {
            return await js.InvokeAsync<bool>(
                "eval", "typeof window.wasmEngine !== 'undefined'");
        }
        catch
        {
            return false;
        }
    }

    // ── Join helpers ──────────────────────────────────────────────────────────

    private ComputedMachine JoinMachine(Machine dynamic)
    {
        MachineStatic staticData = machineService.Machines
            .FirstOrDefault(s => s.Id == dynamic.Id)
            ?? new MachineStatic
            {
                Id = dynamic.Id,
                Name = string.Empty,
                Specialization = MachineSpecialization.Damage,
                TargetType = MachineTargetType.Single,
                Image = string.Empty,
                AbilityKey = string.Empty,
                BaseDamage = 0,
                BaseHealth = 0,
                BaseArmor = 0
            };

        return new ComputedMachine(staticData, dynamic);
    }

    private ComputedHero JoinHero(Hero dynamic)
    {
        HeroStatic staticData = heroService.Heroes
            .FirstOrDefault(s => s.Id == dynamic.Id)
            ?? new HeroStatic
            {
                Id = dynamic.Id,
                Name = string.Empty,
                Type = HeroType.Hero,
                Class = HeroClass.Warrior,
                AttackStyle = HeroAttackStyle.Melee,
                Specialization = HeroSpecialization.Damage,
                Resource = HeroResource.Rage,
                Image = string.Empty,
                BaseDamage = 0,
                BaseHealth = 0,
                BaseArmor = 0,
                AttackSpeed = 0,
                CriticalChance = 0,
                CriticalDamage = 0,
                DodgeChance = 0
            };

        return new ComputedHero(staticData, dynamic);
    }

    // ── WASM path ─────────────────────────────────────────────────────────────

    private async Task<OptimizationResult> RunWasmAsync(OptimizerData data)
    {
        var machines = data.Machines.Select(m =>
        {
            MachineStatic? s = machineService.Machines.FirstOrDefault(x => x.Id == m.Id);
            return new
            {
                id = m.Id,
                is_tank = (s?.Specialization ?? MachineSpecialization.Damage) == MachineSpecialization.Tank,
                is_healer = (s?.Specialization ?? MachineSpecialization.Damage) == MachineSpecialization.Healer,
                base_damage = ToDecimalDto(s?.BaseDamage ?? 0),
                base_health = ToDecimalDto(s?.BaseHealth ?? 0),
                base_armor = ToDecimalDto(s?.BaseArmor ?? 0),
                level = m.Level,
                rarity_level = (int)m.Rarity,
                sacred_level = m.SacredLevel,
                inscription_level = m.InscriptionLevel,
                bp_damage = m.DamageBlueprint,
                bp_health = m.HealthBlueprint,
                bp_armor = m.ArmorBlueprint,
                ability_effect = EncodeEffect(s?.Ability),
                ability_targeting = EncodeTargeting(s?.Ability),
                ability_num_targets = s?.Ability?.NumTargets ?? 0,
                ability_scale_stat = EncodeScaleStat(s?.Ability),
                ability_multiplier = s?.Ability?.Multiplier ?? 0.0,
                overdrive_chance = 0.0,
            };
        }).ToList();

        var config = new
        {
            engineer_level = data.EngineerLevel,
            scarab_level = data.ScarabLevel,
            global_rarity_levels = data.Machines.Sum(m => (int)m.Rarity),
            rift_rank = (int)data.ChaosRiftRank,
            max_mission = 90,
            monte_carlo_simulations = 10_000,
            max_crew_slots = GetMaxCrewSlots(data.EngineerLevel),
            reoptimize_interval = 5,
            artifacts = data.Artifacts
                .Where(a => a.Count > 0)
                .Select(a => new
                {
                    stat = (int)a.Stat,
                    percentage = (double)a.Percentage,
                    count = a.Count,
                }),
            hero_scoring_campaign_tank = new
            {
                damage = data.HeroWeights.CampaignTankDamage,
                health = data.HeroWeights.CampaignTankHealth,
                armor = data.HeroWeights.CampaignTankArmor,
            },
            hero_scoring_campaign_dps = new
            {
                damage = data.HeroWeights.CampaignDpsDamage,
                health = data.HeroWeights.CampaignDpsHealth,
                armor = data.HeroWeights.CampaignDpsArmor,
            },
            hero_scoring_arena_tank = new
            {
                damage = data.HeroWeights.ArenaTankDamage,
                health = data.HeroWeights.ArenaTankHealth,
                armor = data.HeroWeights.ArenaTankArmor,
            },
            hero_scoring_arena_dps = new
            {
                damage = data.HeroWeights.ArenaDpsDamage,
                health = data.HeroWeights.ArenaDpsHealth,
                armor = data.HeroWeights.ArenaDpsArmor,
            },
            heroes = data.Heroes
                .Where(h => h.DamagePercentage > 0 || h.HealthPercentage > 0 || h.ArmorPercentage > 0)
                .OrderByDescending(h => h.DamagePercentage + h.HealthPercentage)
                .Select(h => new
                {
                    id = h.Id,
                    damage_pct = (double)h.DamagePercentage,
                    health_pct = (double)h.HealthPercentage,
                    armor_pct = (double)h.ArmorPercentage,
                }),
        };

        string fn = data.OptimizeMode == OptimizeMode.Arena
            ? "window.wasmEngine.optimize_arena"
            : "window.wasmEngine.optimize_campaign";

        JsonElement json = await js.InvokeAsync<JsonElement>(fn, machines, config);
        return MapWasmResult(json, data);
    }

    // ── C# fallback path ──────────────────────────────────────────────────────

    private OptimizationResult RunCSharp(OptimizerData data)
    {
        List<ComputedMachine> machines = data.Machines.Select(JoinMachine).ToList();
        List<ComputedHero> heroes = data.Heroes.Select(JoinHero).ToList();

        OptimizerInput input = new()
        {
            Machines = machines,
            Heroes = heroes,
            Artifacts = data.Artifacts,
            EngineerLevel = data.EngineerLevel,
            ScarabLevel = data.ScarabLevel,
            RiftRank = data.ChaosRiftRank,
            GlobalRarityLevels = Calculator.GetGlobalRarityLevels(data.Machines),
            HeroWeights = data.HeroWeights,
        };

        Optimizer.Optimizer optimizer = new(input);

        return data.OptimizeMode == OptimizeMode.Arena
            ? MapCSharpResult(optimizer.OptimizeArena())
            : MapCSharpResult(optimizer.OptimizeCampaign());
    }

    // ── Ability encoding ──────────────────────────────────────────────────────

    private static byte EncodeEffect(Ability? a)
    {
        return a is null ? (byte)0 : a.Effect switch
        {
            AbilityEffect.Damage => (byte)1,
            AbilityEffect.Heal => (byte)2,
            _ => (byte)0,
        };
    }

    private static byte EncodeTargeting(Ability? a)
    {
        return a?.TargetPosition switch
        {
            AbilityTargetPosition.Random => 0,
            AbilityTargetPosition.All => 1,
            AbilityTargetPosition.Lowest => 2,
            AbilityTargetPosition.Last => 3,
            AbilityTargetPosition.Self => 4,
            _ => 0,
        };
    }

    private static byte EncodeScaleStat(Ability? a)
    {
        return a?.ScaleStat == AbilityScaleStat.Health ? (byte)1 : (byte)0;
    }

    private static int GetMaxCrewSlots(int engineerLevel)
    {
        return engineerLevel switch
        {
            >= 60 => 6,
            >= 30 => 5,
            _ => 4,
        };
    }

    private static object ToDecimalDto(double value)
    {
        return new
        {
            sign = value >= 0 ? 1 : -1,
            layer = 0,
            mag = Math.Abs(value),
        };
    }

    // ── WASM result mapping ───────────────────────────────────────────────────

    private OptimizationResult MapWasmResult(JsonElement json, OptimizerData data)
    {
        List<ComputedMachine> formation = json.GetProperty("formation")
            .EnumerateArray()
            .Select(m =>
            {
                int id = m.GetProperty("id").GetInt32();
                Machine dynamic = data.Machines.FirstOrDefault(x => x.Id == id) ?? new Machine { Id = id };
                ComputedMachine computed = JoinMachine(dynamic);

                computed.BattleStats = new MachineStats
                {
                    Damage = DecimalDtoToBigDouble(m.GetProperty("battle_damage")),
                    Health = DecimalDtoToBigDouble(m.GetProperty("battle_health")),
                    Armor = DecimalDtoToBigDouble(m.GetProperty("battle_armor")),
                };
                computed.ArenaStats = new MachineStats
                {
                    Damage = DecimalDtoToBigDouble(m.GetProperty("arena_damage")),
                    Health = DecimalDtoToBigDouble(m.GetProperty("arena_health")),
                    Armor = DecimalDtoToBigDouble(m.GetProperty("arena_armor")),
                };
                computed.Crew = m.GetProperty("assigned_hero_ids")
                    .EnumerateArray()
                    .Select(x =>
                    {
                        int heroId = x.GetInt32();
                        Hero heroDynamic = data.Heroes.FirstOrDefault(h => h.Id == heroId)
                            ?? new Hero { Id = heroId };
                        return JoinHero(heroDynamic);
                    })
                    .ToList();

                return computed;
            })
            .ToList();

        Dictionary<CampaignDifficulty, int> lastCleared = [];
        if (json.TryGetProperty("last_cleared", out JsonElement lc))
        {
            lastCleared[CampaignDifficulty.Easy] = lc.GetProperty("easy").GetInt32();
            lastCleared[CampaignDifficulty.Normal] = lc.GetProperty("normal").GetInt32();
            lastCleared[CampaignDifficulty.Hard] = lc.GetProperty("hard").GetInt32();
            lastCleared[CampaignDifficulty.Insane] = lc.GetProperty("insane").GetInt32();
            lastCleared[CampaignDifficulty.Nightmare] = lc.GetProperty("nightmare").GetInt32();
        }

        return new OptimizationResult
        {
            IsArena = data.OptimizeMode == OptimizeMode.Arena,
            Formation = formation,
            BattlePower = DecimalDtoToBigDouble(json.GetProperty("battle_power")),
            ArenaPower = DecimalDtoToBigDouble(json.GetProperty("arena_power")),
            TotalStars = json.TryGetProperty("total_stars", out JsonElement stars) ? stars.GetInt32() : 0,
            LastCleared = lastCleared,
        };
    }

    // ── C# result mapping ─────────────────────────────────────────────────────

    private static OptimizationResult MapCSharpResult(CampaignResult r)
    {
        return new()
        {
            IsArena = false,
            Formation = [.. r.Formation],
            BattlePower = r.BattlePower,
            ArenaPower = r.ArenaPower,
            TotalStars = r.TotalStars,
            LastCleared = r.LastCleared,
        };
    }

    private static OptimizationResult MapCSharpResult(ArenaResult r)
    {
        return new()
        {
            IsArena = true,
            Formation = [.. r.Formation],
            BattlePower = r.BattlePower,
            ArenaPower = r.ArenaPower,
        };
    }

    // ── DecimalDto → BigDouble ────────────────────────────────────────────────

    private static BigDouble DecimalDtoToBigDouble(JsonElement dto)
    {
        long sign = dto.GetProperty("sign").GetInt64();
        long layer = dto.GetProperty("layer").GetInt64();
        double mag = dto.GetProperty("mag").GetDouble();
        return BigDouble.fromComponents((int)sign, (int)layer, mag);
    }
}

/// <summary>
/// View-model consumed by the results UI.
/// Matches the shape returned by both the WASM and C# optimizer paths.
/// </summary>
public sealed class OptimizationResult
{
    public bool IsArena { get; init; }
    public List<ComputedMachine> Formation { get; init; } = [];
    public BigDouble BattlePower { get; init; }
    public BigDouble ArenaPower { get; init; }
    public int TotalStars { get; init; }
    public Dictionary<CampaignDifficulty, int> LastCleared { get; init; } = [];
}