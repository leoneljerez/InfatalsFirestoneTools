using InfatalsFirestoneTools.Models;
using Magic.IndexedDb;

namespace InfatalsFirestoneTools.Services
{
    public class ProfileService(IMagicIndexedDb magicIndexedDb, OptimizerDataFactory optimizerDataFactory, MachineService machineService, HeroService heroService)
    {
        // ── Store accessors ───────────────────────────────────────────────────

        private async Task<IMagicQuery<Profile>> Profiles()
        {
            return await magicIndexedDb.Query<Profile>();
        }

        private async Task<IMagicQuery<ProfileMachine>> Machines()
        {
            return await magicIndexedDb.Query<ProfileMachine>();
        }

        private async Task<IMagicQuery<ProfileHero>> Heroes()
        {
            return await magicIndexedDb.Query<ProfileHero>();
        }

        private async Task<IMagicQuery<ProfileArtifact>> Artifacts()
        {
            return await magicIndexedDb.Query<ProfileArtifact>();
        }

        // ── Public API ────────────────────────────────────────────────────────

        public async Task EnsureDefaultProfileAsync()
        {
            List<Profile> all = await (await Profiles()).ToListAsync();
            if (all.Count == 0)
                _ = await CreateProfileAsync("Default", setActive: true);
        }

        public async Task<List<Profile>> GetAllProfilesAsync()
        {
            return await (await Profiles()).ToListAsync();
        }

        public async Task<(Profile Profile, OptimizerData Data)?> GetActiveProfileAsync()
        {
            Profile? profile = (await GetAllProfilesAsync()).FirstOrDefault(x => x.IsActive);
            if (profile is null) return null;

            OptimizerData data = await LoadOptimizerDataAsync(profile.Id);
            return (profile, data);
        }

        public async Task<(Profile Profile, OptimizerData Data)?> GetProfileAsync(int id)
        {
            Profile? profile = await (await Profiles())
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (profile is null) return null;

            OptimizerData data = await LoadOptimizerDataAsync(id);
            return (profile, data);
        }

        public async Task<Profile> CreateProfileAsync(string name, bool setActive = false)
        {
            DateTime now = DateTime.UtcNow;

            Profile profile = new()
            {
                Name = name,
                IsActive = setActive,
                CreatedAt = now,
                UpdatedAt = now,
            };

            await (await Profiles()).AddAsync(profile);

            // IndexedDB doesn't return the auto-incremented key from AddAsync,
            // so we fetch back the most-recently created profile with this name.
            Profile saved = (await (await Profiles()).ToListAsync())
                .Where(x => x.Name == name)
                .MaxBy(x => x.CreatedAt)!;

            OptimizerData defaults = optimizerDataFactory.Create();

            List<ProfileMachine> machines = defaults.Machines
                .Select(m => new ProfileMachine
                {
                    ProfileId = saved.Id,
                    MachineId = m.Id,
                    Rarity = m.Rarity,
                    Level = m.Level,
                    DamageBlueprint = m.DamageBlueprint,
                    HealthBlueprint = m.HealthBlueprint,
                    ArmorBlueprint = m.ArmorBlueprint,
                    InscriptionLevel = m.InscriptionLevel,
                    SacredLevel = m.SacredLevel,
                })
                .ToList();

            List<ProfileHero> heroes = defaults.Heroes
                .Select(h => new ProfileHero
                {
                    ProfileId = saved.Id,
                    HeroId = h.Id,
                    DamagePercentage = h.DamagePercentage,
                    HealthPercentage = h.HealthPercentage,
                    ArmorPercentage = h.ArmorPercentage,
                })
                .ToList();

            List<ProfileArtifact> artifacts = defaults.Artifacts
                .Select(a => new ProfileArtifact
                {
                    ProfileId = saved.Id,
                    Stat = a.Stat,
                    Percentage = a.Percentage,
                    Count = a.Count,
                })
                .ToList();

            await Task.WhenAll(
                (await Machines()).AddRangeAsync(machines),
                (await Heroes()).AddRangeAsync(heroes),
                (await Artifacts()).AddRangeAsync(artifacts)
            );

            return saved;
        }

        public async Task SetActiveProfileAsync(int id)
        {
            List<Profile> all = await GetAllProfilesAsync();
            foreach (Profile p in all)
                p.IsActive = p.Id == id;

            _ = await (await Profiles()).UpdateRangeAsync(all);
        }

        public async Task SaveProfileAsync(Profile profile, OptimizerData data)
        {
            // Kick off the three DB reads immediately, before any CPU work.
            Task<List<ProfileMachine>> machinesTask = (await Machines())
                .Where(x => x.ProfileId == profile.Id)
                .ToListAsync();

            Task<List<ProfileHero>> heroesTask = (await Heroes())
                .Where(x => x.ProfileId == profile.Id)
                .ToListAsync();

            Task<List<ProfileArtifact>> artifactsTask = (await Artifacts())
                .Where(x => x.ProfileId == profile.Id)
                .ToListAsync();

            // Update the profile object while the DB reads are in flight.
            profile.UpdatedAt = DateTime.UtcNow;
            profile.EngineerLevel = data.EngineerLevel;
            profile.ScarabLevel = data.ScarabLevel;
            profile.ChaosRiftRank = data.ChaosRiftRank;
            profile.OptimizeMode = data.OptimizeMode;
            profile.HeroWeights.CampaignTankDamage = data.HeroWeights.CampaignTankDamage;
            profile.HeroWeights.CampaignTankHealth = data.HeroWeights.CampaignTankHealth;
            profile.HeroWeights.CampaignTankArmor = data.HeroWeights.CampaignTankArmor;
            profile.HeroWeights.CampaignDpsDamage = data.HeroWeights.CampaignDpsDamage;
            profile.HeroWeights.CampaignDpsHealth = data.HeroWeights.CampaignDpsHealth;
            profile.HeroWeights.CampaignDpsArmor = data.HeroWeights.CampaignDpsArmor;
            profile.HeroWeights.ArenaTankDamage = data.HeroWeights.ArenaTankDamage;
            profile.HeroWeights.ArenaTankHealth = data.HeroWeights.ArenaTankHealth;
            profile.HeroWeights.ArenaTankArmor = data.HeroWeights.ArenaTankArmor;
            profile.HeroWeights.ArenaDpsDamage = data.HeroWeights.ArenaDpsDamage;
            profile.HeroWeights.ArenaDpsHealth = data.HeroWeights.ArenaDpsHealth;
            profile.HeroWeights.ArenaDpsArmor = data.HeroWeights.ArenaDpsArmor;

            // Wait for all reads to finish before we mutate the rows.
            await Task.WhenAll(machinesTask, heroesTask, artifactsTask);

            Dictionary<int, ProfileMachine> machineDict =
                machinesTask.Result.ToDictionary(x => x.MachineId);

            Dictionary<int, ProfileHero> heroDict =
                heroesTask.Result.ToDictionary(x => x.HeroId);

            Dictionary<string, ProfileArtifact> artifactDict =
                artifactsTask.Result.ToDictionary(x => $"{x.Stat}-{x.Percentage}");

            foreach (Machine m in data.Machines)
            {
                if (!machineDict.TryGetValue(m.Id, out ProfileMachine? row)) continue;
                row.Rarity = m.Rarity;
                row.Level = m.Level;
                row.DamageBlueprint = m.DamageBlueprint;
                row.HealthBlueprint = m.HealthBlueprint;
                row.ArmorBlueprint = m.ArmorBlueprint;
                row.InscriptionLevel = m.InscriptionLevel;
                row.SacredLevel = m.SacredLevel;
            }

            foreach (Hero h in data.Heroes)
            {
                if (!heroDict.TryGetValue(h.Id, out ProfileHero? row)) continue;
                row.DamagePercentage = h.DamagePercentage;
                row.HealthPercentage = h.HealthPercentage;
                row.ArmorPercentage = h.ArmorPercentage;
            }

            foreach (Artifact a in data.Artifacts)
            {
                if (!artifactDict.TryGetValue($"{a.Stat}-{a.Percentage}", out ProfileArtifact? row)) continue;
                row.Count = a.Count;
            }

            _ = await Task.WhenAll(
                (await Profiles()).UpdateAsync(profile),
                (await Machines()).UpdateRangeAsync(machinesTask.Result),
                (await Heroes()).UpdateRangeAsync(heroesTask.Result),
                (await Artifacts()).UpdateRangeAsync(artifactsTask.Result)
            );
        }

        public async Task DeleteProfileAsync(int id)
        {
            Profile? profile = await (await Profiles())
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (profile is null) return;

            // Fetch all child rows and delete the profile header in parallel.
            Task deleteProfTask = (await Profiles()).DeleteAsync(profile);
            Task<List<ProfileMachine>> machinesTask = (await Machines())
                .Where(x => x.ProfileId == id)
                .ToListAsync();
            Task<List<ProfileHero>> heroesTask = (await Heroes())
                .Where(x => x.ProfileId == id)
                .ToListAsync();
            Task<List<ProfileArtifact>> artifactsTask = (await Artifacts())
                .Where(x => x.ProfileId == id)
                .ToListAsync();

            await Task.WhenAll(deleteProfTask, machinesTask, heroesTask, artifactsTask);

            _ = await Task.WhenAll(
                (await Machines()).DeleteRangeAsync(machinesTask.Result),
                (await Heroes()).DeleteRangeAsync(heroesTask.Result),
                (await Artifacts()).DeleteRangeAsync(artifactsTask.Result)
            );
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<OptimizerData> LoadOptimizerDataAsync(int profileId)
        {
            // All four reads are independent — run them concurrently.
            Task<Profile?> profileTask = (await Profiles())
                .Where(x => x.Id == profileId)
                .FirstOrDefaultAsync();

            Task<List<ProfileMachine>> machinesTask = (await Machines())
                .Where(x => x.ProfileId == profileId)
                .ToListAsync();

            Task<List<ProfileHero>> heroesTask = (await Heroes())
                .Where(x => x.ProfileId == profileId)
                .ToListAsync();

            Task<List<ProfileArtifact>> artifactsTask = (await Artifacts())
                .Where(x => x.ProfileId == profileId)
                .ToListAsync();

            await Task.WhenAll(profileTask, machinesTask, heroesTask, artifactsTask);

            Profile? profile = profileTask.Result;

            Dictionary<int, ProfileMachine> machineMap =
                machinesTask.Result.ToDictionary(x => x.MachineId);

            Dictionary<int, ProfileHero> heroMap =
                heroesTask.Result.ToDictionary(x => x.HeroId);

            List<Machine> machines = machineService.Machines
                .Select(s =>
                {
                    _ = machineMap.TryGetValue(s.Id, out ProfileMachine? row);
                    return new Machine
                    {
                        Id = s.Id,
                        Rarity = row?.Rarity ?? MachineRarity.Common,
                        Level = row?.Level ?? 0,
                        DamageBlueprint = row?.DamageBlueprint ?? 0,
                        HealthBlueprint = row?.HealthBlueprint ?? 0,
                        ArmorBlueprint = row?.ArmorBlueprint ?? 0,
                        InscriptionLevel = row?.InscriptionLevel ?? 0,
                        SacredLevel = row?.SacredLevel ?? 0,
                    };
                })
                .ToList();

            List<Hero> heroes = heroService.Heroes
                .Select(s =>
                {
                    _ = heroMap.TryGetValue(s.Id, out ProfileHero? row);
                    return new Hero
                    {
                        Id = s.Id,
                        DamagePercentage = row?.DamagePercentage ?? 0,
                        HealthPercentage = row?.HealthPercentage ?? 0,
                        ArmorPercentage = row?.ArmorPercentage ?? 0,
                    };
                })
                .ToList();

            List<Artifact> artifacts = artifactsTask.Result
                .Select(a => new Artifact
                {
                    Stat = a.Stat,
                    Percentage = a.Percentage,
                    Count = a.Count,
                })
                .ToList();

            return new OptimizerData
            {
                Machines = machines,
                Heroes = heroes,
                Artifacts = artifacts,
                EngineerLevel = profile?.EngineerLevel ?? 0,
                ScarabLevel = profile?.ScarabLevel ?? 0,
                ChaosRiftRank = profile?.ChaosRiftRank ?? ChaosRiftRank.Bronze,
                OptimizeMode = profile?.OptimizeMode ?? OptimizeMode.Campaign,
                HeroWeights = profile?.HeroWeights ?? new HeroWeights(),
            };
        }
    }
}