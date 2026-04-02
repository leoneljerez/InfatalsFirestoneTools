using InfatalsFirestoneTools.Models;
using InfatalsFirestoneTools.Services;

namespace InfatalsFirestoneTools.Tests.Services;

public class MachineServiceTests
{
    private readonly MachineService _sut;

    public MachineServiceTests()
    {
        _sut = new MachineService(new AbilityService());
    }

    [Fact]
    public void Machines_ContainsExpectedCount()
    {
        Assert.Equal(13, _sut.Machines.Count);
    }

    [Fact]
    public void Machines_AllHaveUniqueIds()
    {
        List<int> ids = _sut.Machines.Select(m => m.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Machines_AllHaveNonEmptyNames()
    {
        Assert.All(_sut.Machines, m => Assert.NotEmpty(m.Name));
    }

    [Fact]
    public void Machines_AllHavePositiveBaseStats()
    {
        Assert.All(_sut.Machines, m =>
        {
            Assert.True(m.BaseDamage > 0, $"{m.Name} has zero base damage");
            Assert.True(m.BaseHealth > 0, $"{m.Name} has zero base health");
            Assert.True(m.BaseArmor > 0, $"{m.Name} has zero base armor");
        });
    }

    [Fact]
    public void Machines_AbilitiesAreAttached()
    {
        Assert.All(_sut.Machines, m =>
            Assert.NotNull(m.Ability));
    }

    [Fact]
    public void Machines_ContainsTanksAndDpsAndHealers()
    {
        List<MachineSpecialization> specs = _sut.Machines.Select(m => m.Specialization).Distinct().ToList();
        Assert.Contains(MachineSpecialization.Tank, specs);
        Assert.Contains(MachineSpecialization.Damage, specs);
        Assert.Contains(MachineSpecialization.Healer, specs);
    }

    [Fact]
    public void Machines_GoliathExists()
    {
        Assert.Contains(_sut.Machines, m => m.Name == "GoliathLabel");
    }

    [Fact]
    public void Machines_GoliathIsTank()
    {
        MachineStatic goliath = _sut.Machines.First(m => m.Name == "GoliathLabel");
        Assert.Equal(MachineSpecialization.Tank, goliath.Specialization);
    }

    [Fact]
    public void Machines_AllHaveValidImages()
    {
        Assert.All(_sut.Machines, m =>
        {
            Assert.NotEmpty(m.Image);
            Assert.StartsWith("img/", m.Image);
        });
    }
}

public class HeroServiceTests
{
    private readonly HeroService _sut = new();

    [Fact]
    public void Heroes_ContainsExpectedCount()
    {
        Assert.Equal(37, _sut.Heroes.Count);
    }

    [Fact]
    public void Heroes_AllHaveUniqueIds()
    {
        List<int> ids = _sut.Heroes.Select(h => h.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Heroes_AllHaveNonEmptyNames()
    {
        Assert.All(_sut.Heroes, h => Assert.NotEmpty(h.Name));
    }

    [Fact]
    public void Heroes_AllHavePositiveBaseStats()
    {
        Assert.All(_sut.Heroes, h =>
        {
            Assert.True(h.BaseDamage > 0, $"{h.Name} has zero damage");
            Assert.True(h.BaseHealth > 0, $"{h.Name} has zero health");
            Assert.True(h.BaseArmor > 0, $"{h.Name} has zero armor");
        });
    }

    [Fact]
    public void Heroes_ContainAllSpecializations()
    {
        List<HeroSpecialization> specs = _sut.Heroes.Select(h => h.Specialization).Distinct().ToList();
        Assert.Contains(HeroSpecialization.Tank, specs);
        Assert.Contains(HeroSpecialization.Damage, specs);
        Assert.Contains(HeroSpecialization.Healer, specs);
    }

    [Fact]
    public void Heroes_ContainAllTypes()
    {
        List<HeroType> types = _sut.Heroes.Select(h => h.Type).Distinct().ToList();
        Assert.Contains(HeroType.Hero, types);
        Assert.Contains(HeroType.Mercenary, types);
        Assert.Contains(HeroType.God, types);
    }

    [Fact]
    public void Heroes_GodHeroesUseGodClass()
    {
        List<HeroStatic> gods = _sut.Heroes.Where(h => h.Type == HeroType.God).ToList();
        Assert.NotEmpty(gods);
        Assert.All(gods, h => Assert.Equal(HeroClass.God, h.Class));
    }

    [Fact]
    public void Heroes_AllHaveValidImages()
    {
        Assert.All(_sut.Heroes, h =>
        {
            Assert.NotEmpty(h.Image);
            Assert.StartsWith("img/", h.Image);
        });
    }

    [Fact]
    public void Heroes_CriticalChancesAreInValidRange()
    {
        Assert.All(_sut.Heroes, h =>
            Assert.InRange(h.CriticalChance, 0.0, 1.0));
    }
}

public class AbilityServiceTests
{
    private readonly AbilityService _sut = new();

    [Fact]
    public void Abilities_ContainsExpectedKeys()
    {
        Assert.Contains("dmg_1x_150", _sut.Abilities.Keys);
        Assert.Contains("heal_all_150", _sut.Abilities.Keys);
        Assert.Contains("heal_self_hp_10", _sut.Abilities.Keys);
    }

    [Fact]
    public void Abilities_DamageAbilitiesHaveDamageEffect()
    {
        IEnumerable<KeyValuePair<string, Ability>> dmgAbilities = _sut.Abilities.Where(kv => kv.Key.StartsWith("dmg_"));
        Assert.All(dmgAbilities, kv => Assert.Equal(AbilityEffect.Damage, kv.Value.Effect));
    }

    [Fact]
    public void Abilities_HealAbilitiesHaveHealEffect()
    {
        IEnumerable<KeyValuePair<string, Ability>> healAbilities = _sut.Abilities.Where(kv => kv.Key.StartsWith("heal_"));
        Assert.All(healAbilities, kv => Assert.Equal(AbilityEffect.Heal, kv.Value.Effect));
    }

    [Fact]
    public void Abilities_AllHavePositiveMultiplier()
    {
        Assert.All(_sut.Abilities.Values, a => Assert.True(a.Multiplier > 0));
    }

    [Fact]
    public void Abilities_AllHaveDescriptions()
    {
        Assert.All(_sut.Abilities.Values, a => Assert.NotEmpty(a.Description));
    }
}