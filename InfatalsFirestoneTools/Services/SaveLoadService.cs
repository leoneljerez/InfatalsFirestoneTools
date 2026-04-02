using InfatalsFirestoneTools.Models;
using InfatalsFirestoneTools.Resources;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InfatalsFirestoneTools.Services
{
    public static class SaveLoadService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // ── Export ───────────────────────────────────────────────────────────

        public static string Export(OptimizerData data)
        {
            SaveFile save = new()
            {
                Version = 2,
                AppVersion = PatchNotes.Current.Version,
                General = new SaveGeneral
                {
                    EngineerLevel = data.EngineerLevel,
                    ScarabLevel = data.ScarabLevel,
                    RiftRank = data.ChaosRiftRank,
                    OptimizeMode = data.OptimizeMode,
                    HeroWeights = new HeroWeights
                    {
                        CampaignTankDamage = data.HeroWeights.CampaignTankDamage,
                        CampaignTankHealth = data.HeroWeights.CampaignTankHealth,
                        CampaignTankArmor = data.HeroWeights.CampaignTankArmor,
                        CampaignDpsDamage = data.HeroWeights.CampaignDpsDamage,
                        CampaignDpsHealth = data.HeroWeights.CampaignDpsHealth,
                        CampaignDpsArmor = data.HeroWeights.CampaignDpsArmor,
                        ArenaTankDamage = data.HeroWeights.ArenaTankDamage,
                        ArenaTankHealth = data.HeroWeights.ArenaTankHealth,
                        ArenaTankArmor = data.HeroWeights.ArenaTankArmor,
                        ArenaDpsDamage = data.HeroWeights.ArenaDpsDamage,
                        ArenaDpsHealth = data.HeroWeights.ArenaDpsHealth,
                        ArenaDpsArmor = data.HeroWeights.ArenaDpsArmor,
                    }
                },
                Machines = data.Machines.Select(m => new SaveMachine
                {
                    Id = m.Id,
                    Rarity = m.Rarity,
                    Level = m.Level,
                    DamageBlueprint = m.DamageBlueprint,
                    HealthBlueprint = m.HealthBlueprint,
                    ArmorBlueprint = m.ArmorBlueprint,
                    InscriptionLevel = m.InscriptionLevel,
                    SacredLevel = m.SacredLevel,
                }).ToList(),
                Heroes = data.Heroes.Select(h => new SaveHero
                {
                    Id = h.Id,
                    DamagePercentage = h.DamagePercentage,
                    HealthPercentage = h.HealthPercentage,
                    ArmorPercentage = h.ArmorPercentage,
                }).ToList(),
                Artifacts = data.Artifacts.Select(a => new SaveArtifact
                {
                    Stat = a.Stat,
                    Percentage = a.Percentage,
                    Count = a.Count,
                }).ToList(),
            };

            return JsonSerializer.Serialize(save, JsonOptions);
        }

        // ── Import ───────────────────────────────────────────────────────────

        public static ImportResult Import(string json, OptimizerData current)
        {
            if (string.IsNullOrWhiteSpace(json))
                return ImportResult.Fail(LanguageResource.SaveDataEmpty);

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                int version = doc.RootElement.TryGetProperty("version", out JsonElement v)
                    ? v.GetInt32()
                    : 1;

                return version switch
                {
                    1 => ImportV1(doc.RootElement, current),
                    2 => ImportV2(json, current),
                    _ => ImportResult.Fail($"{LanguageResource.UnknownSaveVersion}: {version}")
                };
            }
            catch (JsonException ex)
            {
                return ImportResult.Fail($"{LanguageResource.InvalidJSON}: {ex.Message}");
            }
        }

        // ── V2 import (native format) ────────────────────────────────────────

        private static ImportResult ImportV2(string json, OptimizerData current)
        {
            SaveFile? save = JsonSerializer.Deserialize<SaveFile>(json, JsonOptions);
            if (save is null)
                return ImportResult.Fail(LanguageResource.InvalidSaveData);

            // General
            current.EngineerLevel = save.General.EngineerLevel;
            current.ScarabLevel = save.General.ScarabLevel;
            current.ChaosRiftRank = save.General.RiftRank;
            current.OptimizeMode = save.General.OptimizeMode;
            current.HeroWeights.CampaignTankDamage = save.General.HeroWeights.CampaignTankDamage;
            current.HeroWeights.CampaignTankHealth = save.General.HeroWeights.CampaignTankHealth;
            current.HeroWeights.CampaignTankArmor = save.General.HeroWeights.CampaignTankArmor;
            current.HeroWeights.CampaignDpsDamage = save.General.HeroWeights.CampaignDpsDamage;
            current.HeroWeights.CampaignDpsHealth = save.General.HeroWeights.CampaignDpsHealth;
            current.HeroWeights.CampaignDpsArmor = save.General.HeroWeights.CampaignDpsArmor;
            current.HeroWeights.ArenaTankDamage = save.General.HeroWeights.ArenaTankDamage;
            current.HeroWeights.ArenaTankHealth = save.General.HeroWeights.ArenaTankHealth;
            current.HeroWeights.ArenaTankArmor = save.General.HeroWeights.ArenaTankArmor;
            current.HeroWeights.ArenaDpsDamage = save.General.HeroWeights.ArenaDpsDamage;
            current.HeroWeights.ArenaDpsHealth = save.General.HeroWeights.ArenaDpsHealth;
            current.HeroWeights.ArenaDpsArmor = save.General.HeroWeights.ArenaDpsArmor;

            // Machines
            foreach (SaveMachine m in save.Machines)
            {
                Machine? machine = current.Machines.FirstOrDefault(x => x.Id == m.Id);
                if (machine is null) continue;

                machine.Rarity = m.Rarity;
                machine.Level = m.Level;
                machine.DamageBlueprint = m.DamageBlueprint;
                machine.HealthBlueprint = m.HealthBlueprint;
                machine.ArmorBlueprint = m.ArmorBlueprint;
                machine.InscriptionLevel = m.InscriptionLevel;
                machine.SacredLevel = m.SacredLevel;
            }

            // Heroes
            foreach (SaveHero h in save.Heroes)
            {
                Hero? hero = current.Heroes.FirstOrDefault(x => x.Id == h.Id);
                if (hero is null) continue;

                hero.DamagePercentage = h.DamagePercentage;
                hero.HealthPercentage = h.HealthPercentage;
                hero.ArmorPercentage = h.ArmorPercentage;
            }

            // Artifacts
            foreach (SaveArtifact a in save.Artifacts)
            {
                Artifact? artifact = current.Artifacts.FirstOrDefault(x =>
                    x.Stat == a.Stat &&
                    x.Percentage == a.Percentage);

                if (artifact is not null)
                    artifact.Count = a.Count;
            }

            return ImportResult.Ok();
        }

        // ── V1 import (legacy website format) ───────────────────────────────

        private static ImportResult ImportV1(JsonElement root, OptimizerData current)
        {
            if (root.TryGetProperty("general", out JsonElement general))
            {
                if (general.TryGetProperty("engineerLevel", out JsonElement el))
                    current.EngineerLevel = el.GetInt32();

                if (general.TryGetProperty("scarabLevel", out JsonElement sl))
                    current.ScarabLevel = sl.GetInt32();

                if (general.TryGetProperty("riftRank", out JsonElement rr))
                    if (TryParseEnum<ChaosRiftRank>(rr.GetString(), out ChaosRiftRank rank))
                        current.ChaosRiftRank = rank;
            }

            if (root.TryGetProperty("machines", out JsonElement machines))
                foreach (JsonElement m in machines.EnumerateArray())
                    ApplyMachineV1(m, current.Machines);

            if (root.TryGetProperty("heroes", out JsonElement heroes))
                foreach (JsonElement h in heroes.EnumerateArray())
                    ApplyHeroV1(h, current.Heroes);

            if (root.TryGetProperty("artifacts", out JsonElement artifacts))
                ApplyArtifactsV1(artifacts, current.Artifacts);

            return ImportResult.Ok(LanguageResource.MigratedSave);
        }

        private static void ApplyMachineV1(JsonElement el, List<Machine> machines)
        {
            if (!el.TryGetProperty("id", out JsonElement idEl)) return;
            Machine? machine = machines.FirstOrDefault(m => m.Id == idEl.GetInt32());
            if (machine is null) return;

            if (el.TryGetProperty("rarity", out JsonElement r))
                if (TryParseEnum<MachineRarity>(r.GetString(), out MachineRarity rarity))
                    machine.Rarity = rarity;

            if (el.TryGetProperty("level", out JsonElement l))
                machine.Level = l.GetInt32();

            if (el.TryGetProperty("blueprints", out JsonElement bp))
            {
                if (bp.TryGetProperty("damage", out JsonElement d)) machine.DamageBlueprint = d.GetInt32();
                if (bp.TryGetProperty("health", out JsonElement h)) machine.HealthBlueprint = h.GetInt32();
                if (bp.TryGetProperty("armor", out JsonElement a)) machine.ArmorBlueprint = a.GetInt32();
            }

            if (el.TryGetProperty("inscriptionLevel", out JsonElement il))
                machine.InscriptionLevel = il.GetInt32();

            if (el.TryGetProperty("sacredLevel", out JsonElement sl))
                machine.SacredLevel = sl.GetInt32();
        }

        private static void ApplyHeroV1(JsonElement el, List<Hero> heroes)
        {
            if (!el.TryGetProperty("id", out JsonElement idEl)) return;
            Hero? hero = heroes.FirstOrDefault(h => h.Id == idEl.GetInt32());
            if (hero is null) return;

            if (el.TryGetProperty("percentages", out JsonElement pct))
            {
                if (pct.TryGetProperty("damage", out JsonElement d)) hero.DamagePercentage = d.GetInt32();
                if (pct.TryGetProperty("health", out JsonElement h)) hero.HealthPercentage = h.GetInt32();
                if (pct.TryGetProperty("armor", out JsonElement a)) hero.ArmorPercentage = a.GetInt32();
            }
        }

        private static void ApplyArtifactsV1(JsonElement el, List<Artifact> artifacts)
        {
            foreach (JsonProperty statProp in el.EnumerateObject())
            {
                if (!TryParseEnum<ArtifactStat>(statProp.Name, out ArtifactStat stat))
                    continue;

                foreach (JsonProperty pctProp in statProp.Value.EnumerateObject())
                {
                    if (!int.TryParse(pctProp.Name, out int pct)) continue;
                    int count = pctProp.Value.GetInt32();
                    if (count == 0) continue;

                    Artifact? artifact = artifacts.FirstOrDefault(a =>
                        a.Stat == stat &&
                        a.Percentage == pct);

                    if (artifact is not null)
                        artifact.Count = count;
                }
            }
        }

        private static bool TryParseEnum<T>(string? value, out T result) where T : struct, Enum
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                Enum.TryParse<T>(value, true, out result))
            {
                return true;
            }

            result = default;
            return false;
        }
    }

    // ── Save file DTOs ───────────────────────────────────────────────────────

    public sealed class SaveFile
    {
        public int Version { get; set; }
        public string AppVersion { get; set; } = "";
        public SaveGeneral General { get; set; } = new();
        public List<SaveMachine> Machines { get; set; } = [];
        public List<SaveHero> Heroes { get; set; } = [];
        public List<SaveArtifact> Artifacts { get; set; } = [];
    }

    public sealed class SaveGeneral
    {
        public int EngineerLevel { get; set; }
        public int ScarabLevel { get; set; }
        public ChaosRiftRank RiftRank { get; set; } = ChaosRiftRank.Bronze;
        public OptimizeMode OptimizeMode { get; set; } = OptimizeMode.Campaign;
        public HeroWeights HeroWeights { get; set; } = new();
    }

    public sealed class SaveMachine
    {
        public int Id { get; set; }
        public MachineRarity Rarity { get; set; } = MachineRarity.Common;
        public int Level { get; set; }
        public int DamageBlueprint { get; set; }
        public int HealthBlueprint { get; set; }
        public int ArmorBlueprint { get; set; }
        public int InscriptionLevel { get; set; }
        public int SacredLevel { get; set; }
    }

    public sealed class SaveHero
    {
        public int Id { get; set; }
        public int DamagePercentage { get; set; }
        public int HealthPercentage { get; set; }
        public int ArmorPercentage { get; set; }
    }

    public sealed class SaveArtifact
    {
        public ArtifactStat Stat { get; set; }
        public int Percentage { get; set; }
        public int Count { get; set; }
    }

    public sealed class ImportResult
    {
        public bool Success { get; private init; }
        public string Message { get; private init; } = "";

        public static ImportResult Ok(string? message = null)
        {
            return new() { Success = true, Message = message ?? LanguageResource.SaveLoadedSuccessfully };
        }

        public static ImportResult Fail(string message)
        {
            return new() { Success = false, Message = message };
        }
    }
}