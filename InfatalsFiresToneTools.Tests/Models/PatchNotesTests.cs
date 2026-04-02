using InfatalsFirestoneTools.Models;

namespace InfatalsFirestoneTools.Tests.Models;

public class PatchNotesTests
{
    [Fact]
    public void All_IsNotEmpty()
    {
        Assert.NotEmpty(PatchNotes.All);
    }

    [Fact]
    public void Current_IsFirstEntry()
    {
        Assert.Same(PatchNotes.All[0], PatchNotes.Current);
    }

    [Fact]
    public void All_AllVersionsAreNonEmpty()
    {
        Assert.All(PatchNotes.All, n => Assert.NotEmpty(n.Version));
    }

    [Fact]
    public void All_AllHaveAtLeastOneChange()
    {
        Assert.All(PatchNotes.All, n => Assert.NotEmpty(n.Changes));
    }

    [Fact]
    public void All_AllChangesAreNonEmpty()
    {
        Assert.All(PatchNotes.All, n =>
            Assert.All(n.Changes, c => Assert.NotEmpty(c)));
    }

    [Fact]
    public void All_DatesAreValid()
    {
        Assert.All(PatchNotes.All, n =>
        {
            Assert.True(n.Date.Year >= 2024, $"Suspicious year in version {n.Version}");
            Assert.InRange(n.Date.Month, 1, 12);
            Assert.InRange(n.Date.DayOfYear, 1, 366);
        });
    }
}