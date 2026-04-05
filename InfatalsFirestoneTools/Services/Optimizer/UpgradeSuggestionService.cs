using BreakEternity;
using InfatalsFirestoneTools.Models;
using InfatalsFirestoneTools.Resources;

namespace InfatalsFirestoneTools.Services.Optimizer
{
    public class UpgradeSuggestionService(MachineService machineService, OptimizerService optimizerService)
    {
        private const int MaxLevelCap = 500;
        private const int LevelStepSize = 1;     // suggest levels in steps of 1
        private const int MaxBlueprintIterations = 100; // safety cap on the search loop

        public async Task<UpgradeSuggestionResult?> GetSuggestionsAsync(OptimizationResult result, OptimizerData data)
        {
            if (result.IsArena || result.Formation.Count == 0)
                return null;

            // ── Find the next mission that you need more power for ────────────────

            (int targetMission, CampaignDifficulty targetDifficulty) = FindNextTarget(result);

            // ── Identify top tank and top DPS by battle power ─────────────────────
            // Sort the entire formation by power descending, then check if the top
            // two are one tank and one non-tank. If not, warn the user.

            List<ComputedMachine> byPower = result.Formation
                .OrderByDescending(m => Calculator.MachinePower(m.BattleStats).toDouble())
                .ToList();

            ComputedMachine? first = byPower.ElementAtOrDefault(0);
            ComputedMachine? second = byPower.ElementAtOrDefault(1);

            bool topTwoAreMixed = first is not null && second is not null &&
                                  first.Specialization == MachineSpecialization.Tank !=
                                  (second.Specialization == MachineSpecialization.Tank);

            ComputedMachine? topTank = byPower.FirstOrDefault(m =>
                m.Specialization == MachineSpecialization.Tank);

            ComputedMachine? topDps = byPower.FirstOrDefault(m =>
                m.Specialization != MachineSpecialization.Tank);

            bool hasCorrectComposition = topTwoAreMixed;

            string? compositionWarning = null;
            if (!hasCorrectComposition)
            {
                bool hasAnyTank = topTank is not null;
                bool hasAnyDps = topDps is not null;

                compositionWarning = (hasAnyTank, hasAnyDps) switch
                {
                    (false, _) =>
                        LanguageResource.UpgradeSuggestionNoTank,
                    (_, false) =>
                        LanguageResource.UpgradeSuggestionNoDPS,
                    _ =>
                        string.Empty
                };
            }

            // We always suggest on the top tank and top DPS regardless of composition,
            // falling back to the top two by power if one role is missing entirely.
            List<ComputedMachine> targets = new();

            if (topTank is not null)
                targets.Add(topTank);
            else if (first is not null)
                targets.Add(first);

            if (topDps is not null && topDps != topTank)
                targets.Add(topDps);
            else if (second is not null && second != targets.FirstOrDefault())
                targets.Add(second);

            if (targets.Count == 0)
                return null;

            // ── Build suggestions ─────────────────────────────────────────────────

            BigDouble requiredPower = Calculator.RequiredPower(targetMission, targetDifficulty);
            BigDouble currentPower = Calculator.SquadPower(result.Formation, arena: false);

            // Work on mutable copies so we can simulate upgrades
            Dictionary<int, Machine> simulatedMachines = data.Machines
                .Select(m => new Machine
                {
                    Id = m.Id,
                    Rarity = m.Rarity,
                    Level = m.Level,
                    DamageBlueprint = m.DamageBlueprint,
                    HealthBlueprint = m.HealthBlueprint,
                    ArmorBlueprint = m.ArmorBlueprint,
                    InscriptionLevel = m.InscriptionLevel,
                    SacredLevel = m.SacredLevel,
                })
                .ToDictionary(m => m.Id);

            int globalRarityLevels = Calculator.GetGlobalRarityLevels(data.Machines);

            // Iterate: blueprints first (cheap), then levels (expensive)
            int iterations = 0;
            while (currentPower < requiredPower && iterations < MaxBlueprintIterations)
            {
                iterations++;
                bool madeProgress = false;

                foreach (ComputedMachine target in targets)
                {
                    Machine sim = simulatedMachines[target.Id];
                    int bpCap = Calculator.GetMaxBlueprintLevel(sim.Level);

                    // Try adding one blueprint point in the highest-value stat for this role
                    bool blueprintAdded = TryAddBlueprint(sim, bpCap, target.Specialization);

                    if (!blueprintAdded && sim.Level < MaxLevelCap)
                    {
                        // Blueprints are maxed at current level — suggest a level increase
                        sim.Level = Math.Min(sim.Level + LevelStepSize, MaxLevelCap);
                        madeProgress = true;
                    }
                    else if (blueprintAdded)
                    {
                        madeProgress = true;
                    }

                    // Recalculate squad power with the simulated state
                    currentPower = RecalculateSquadPower(
                        result.Formation, simulatedMachines, data, globalRarityLevels);

                    if (currentPower >= requiredPower)
                        break;
                }

                if (!madeProgress) break;
            }

            // ── Build the suggestion records ──────────────────────────────────────

            List<UpgradeSuggestion> suggestions = new();

            foreach (ComputedMachine target in targets)
            {
                Machine original = data.Machines.First(m => m.Id == target.Id);
                Machine sim = simulatedMachines[target.Id];
                MachineStatic? staticData = machineService.Machines
                    .FirstOrDefault(s => s.Id == target.Id);

                if (staticData is null) continue;

                // Only include if something actually changed
                if (sim.Level == original.Level &&
                    sim.DamageBlueprint == original.DamageBlueprint &&
                    sim.HealthBlueprint == original.HealthBlueprint &&
                    sim.ArmorBlueprint == original.ArmorBlueprint)
                    continue;

                suggestions.Add(new UpgradeSuggestion
                {
                    MachineName = staticData.Name,
                    MachineImage = staticData.Image,
                    Specialization = staticData.Specialization,
                    CurrentLevel = original.Level,
                    SuggestedLevel = sim.Level,
                    CurrentDamageBlueprint = original.DamageBlueprint,
                    SuggestedDamageBlueprint = sim.DamageBlueprint,
                    CurrentHealthBlueprint = original.HealthBlueprint,
                    SuggestedHealthBlueprint = sim.HealthBlueprint,
                    CurrentArmorBlueprint = original.ArmorBlueprint,
                    SuggestedArmorBlueprint = sim.ArmorBlueprint
                });
            }

            return new UpgradeSuggestionResult
            {
                Suggestions = suggestions,
                TargetMission = targetMission,
                TargetDifficulty = targetDifficulty,
                HasCorrectComposition = hasCorrectComposition,
                CompositionWarning = compositionWarning
            };
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static (int Mission, CampaignDifficulty Difficulty) FindNextTarget(OptimizationResult result)
        {
            // Walk from the highest cleared point and find the first mission that fails
            CampaignDifficulty[] difficulties = Enum.GetValues<CampaignDifficulty>().OrderBy(d => d).ToArray();

            // Start from the highest cleared difficulty/mission
            CampaignDifficulty highestDiff = difficulties[0];
            int highestMission = 0;

            foreach (CampaignDifficulty diff in difficulties)
            {
                if (result.LastCleared.TryGetValue(diff, out int cleared) && cleared > 0)
                {
                    highestDiff = diff;
                    highestMission = cleared;
                }
            }

            // The next target is one mission beyond the highest cleared
            int nextMission = highestMission + 1;
            CampaignDifficulty nextDiff = highestDiff;

            if (nextMission > 90)
            {
                // Move to the next difficulty
                int diffIdx = Array.IndexOf(difficulties, highestDiff);
                if (diffIdx < difficulties.Length - 1)
                {
                    nextDiff = difficulties[diffIdx + 1];
                    nextMission = 1;
                }
                else
                {
                    // Already at max — target the last mission of the hardest difficulty
                    return (90, difficulties[^1]);
                }
            }

            return (nextMission, nextDiff);
        }

        private static bool TryAddBlueprint(Machine sim, int cap, MachineSpecialization spec)
        {
            if (spec == MachineSpecialization.Tank)
            {
                if (sim.HealthBlueprint < cap) { sim.HealthBlueprint++; return true; }
                if (sim.ArmorBlueprint < cap) { sim.ArmorBlueprint++; return true; }
            }
            else
            {
                if (sim.DamageBlueprint < cap) { sim.DamageBlueprint++; return true; }
                if (sim.HealthBlueprint < cap) { sim.HealthBlueprint++; return true; }
            }
            return false;
        }

        private BigDouble RecalculateSquadPower(IReadOnlyList<ComputedMachine> formation, Dictionary<int, Machine> simulatedMachines, OptimizerData data, int globalRarityLevels)
        {
            BigDouble total = BigDouble.dZero;

            foreach (ComputedMachine m in formation)
            {
                Machine sim = simulatedMachines.TryGetValue(m.Id, out Machine? s) ? s : data.Machines.First(x => x.Id == m.Id);
                MachineStatic? staticData = machineService.Machines.FirstOrDefault(x => x.Id == m.Id);
                if (staticData is null) continue;

                ComputedMachine computed = new(staticData, sim);

                (BigDouble dmg, BigDouble hp, BigDouble arm) = Calculator.CalculateBattleAttributes(
                    computed,
                    m.Crew.Cast<ComputedHero>().ToList(),
                    globalRarityLevels,
                    data.Artifacts,
                    data.EngineerLevel);

                computed.BattleStats = new MachineStats { Damage = dmg, Health = hp, Armor = arm };
                total += Calculator.MachinePower(computed.BattleStats);
            }

            return total;
        }
    }
}
