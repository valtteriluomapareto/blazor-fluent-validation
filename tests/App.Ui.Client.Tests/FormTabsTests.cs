using Bunit;
using FormValidationTest.Client.Components.Forms;

namespace App.Ui.Client.Tests;

public sealed class FormTabsTests : IDisposable
{
    private readonly BunitContext context = new();

    public void Dispose() => context.Dispose();

    [Fact]
    public void FormTabs_renders_buttons_and_step_indicator()
    {
        var cut = context.Render<FormTabs>(parameters =>
            parameters
                .Add(p => p.Tabs, new[] { "Basics", "Details" })
                .Add(p => p.ActiveTab, 0)
                .Add(p => p.ShowStepIndicator, true)
        );

        var buttons = cut.FindAll("button");
        Assert.Equal(2, buttons.Count);
        Assert.Contains("Basics", buttons[0].TextContent);
        Assert.Contains("Details", buttons[1].TextContent);
        Assert.Contains("Step 1 of 2", cut.Markup);
    }

    [Fact]
    public void FormTabs_hides_step_indicator_when_disabled()
    {
        var cut = context.Render<FormTabs>(parameters =>
            parameters
                .Add(p => p.Tabs, new[] { "Basics", "Details" })
                .Add(p => p.ActiveTab, 0)
                .Add(p => p.ShowStepIndicator, false)
        );

        Assert.DoesNotContain("Step 1 of 2", cut.Markup);
    }

    [Fact]
    public void FormTabs_does_not_render_when_tabs_empty()
    {
        var cut = context.Render<FormTabs>(parameters =>
            parameters.Add(p => p.Tabs, Array.Empty<string>())
        );

        Assert.Empty(cut.FindAll("button"));
    }

    [Fact]
    public void FormTabs_invokes_callback_when_new_tab_selected()
    {
        var selected = new List<int>();

        var cut = context.Render<FormTabs>(parameters =>
            parameters
                .Add(p => p.Tabs, new[] { "Basics", "Details" })
                .Add(p => p.ActiveTab, 0)
                .Add(p => p.ActiveTabChanged, value => selected.Add(value))
        );

        cut.FindAll("button")[1].Click();

        var updated = Assert.Single(selected);
        Assert.Equal(1, updated);
    }

    [Fact]
    public void FormTabs_does_not_invoke_callback_for_current_tab()
    {
        var selected = new List<int>();

        var cut = context.Render<FormTabs>(parameters =>
            parameters
                .Add(p => p.Tabs, new[] { "Basics", "Details" })
                .Add(p => p.ActiveTab, 0)
                .Add(p => p.ActiveTabChanged, value => selected.Add(value))
        );

        cut.FindAll("button")[0].Click();

        Assert.Empty(selected);
    }
}
