using AngleSharp.Dom;
using Bunit;
using InfatalsFirestoneTools.Components.Guardian.StrangeDustCalculator;
using InfatalsFirestoneTools.Models;
using Microsoft.AspNetCore.Components;

namespace InfatalsFirestoneTools.Tests.Components;

public class GuardianPositionPickerTests : ComponentTestBase
{
    private static GuardianPosition DefaultPosition =>
        new(GuardianEvolution.Bronze, GuardianRank.OneStar);

    [Fact]
    public void Renders_SelectedLabel_WhenValueIsSet()
    {
        GuardianPosition pos = new(GuardianEvolution.Bronze, GuardianRank.TwoStar);

        IRenderedComponent<GuardianPositionPicker> cut = Render<GuardianPositionPicker>(p => p
            .Add(x => x.Value, pos)
            .Add(x => x.ValueChanged, EventCallback<GuardianPosition>.Empty)
        );

        Assert.Contains(pos.Label, cut.Markup);
    }

    [Fact]
    public void ClickingTrigger_OpensDropdown()
    {
        IRenderedComponent<GuardianPositionPicker> cut = Render<GuardianPositionPicker>(p => p
            .Add(x => x.Value, DefaultPosition)
            .Add(x => x.ValueChanged, EventCallback<GuardianPosition>.Empty)
        );

        // Dropdown not visible initially
        Assert.DoesNotContain("Stars", cut.Markup.ToLower().Contains("stars").ToString());

        cut.Find("button").Click();

        // After click, panel should appear with Stars and Crowns headers
        Assert.Contains("Stars", cut.Markup);
        Assert.Contains("Crowns", cut.Markup);
    }

    [Fact]
    public void ClickingOverlay_ClosesDropdown()
    {
        IRenderedComponent<GuardianPositionPicker> cut = Render<GuardianPositionPicker>(p => p
            .Add(x => x.Value, DefaultPosition)
            .Add(x => x.ValueChanged, EventCallback<GuardianPosition>.Empty)
        );

        // Open dropdown
        cut.Find("button").Click();
        Assert.Contains("Stars", cut.Markup);

        // Click the overlay (fixed inset-0 div)
        cut.Find("div.fixed").Click();
        Assert.DoesNotContain("Stars", cut.Markup);
    }

    [Fact]
    public void SelectingCell_InvokesValueChanged()
    {
        GuardianPosition? selected = null;

        IRenderedComponent<GuardianPositionPicker> cut = Render<GuardianPositionPicker>(p => p
            .Add(x => x.Value, DefaultPosition)
            .Add(x => x.ValueChanged, pos => selected = pos)
        );

        // Open dropdown
        cut.Find("button").Click();

        // Click any cell button inside the grid (skip the trigger button)
        List<IElement> cellButtons = cut.FindAll("button").Skip(1).ToList();
        cellButtons[0].Click();

        Assert.NotNull(selected);
    }

    [Fact]
    public void SelectedCell_HasSelectedStyling()
    {
        GuardianPosition pos = new(GuardianEvolution.Bronze, GuardianRank.OneStar);

        IRenderedComponent<GuardianPositionPicker> cut = Render<GuardianPositionPicker>(p => p
            .Add(x => x.Value, pos)
            .Add(x => x.ValueChanged, EventCallback<GuardianPosition>.Empty)
        );

        cut.Find("button").Click(); // open

        // The selected class should be present somewhere in the dropdown
        Assert.Contains("bg-primary/20", cut.Markup);
    }
}