using App.Contracts;
using FormValidationTest.Client.Components.Forms;
using Microsoft.AspNetCore.Components;

namespace App.Ui.Client.Tests;

public sealed class FormComponentsTests : IDisposable
{
    private readonly BunitContext context = new();

    public void Dispose() => context.Dispose();

    [Fact]
    public void FormCard_renders_child_content()
    {
        var cut = context.Render<FormCard>(parameters =>
            parameters.AddChildContent("<p>Card content</p>")
        );

        Assert.Contains("Card content", cut.Markup);
        Assert.Contains("rounded-xl", cut.Markup);
    }

    [Fact]
    public void FormField_renders_label_and_help_text()
    {
        var cut = context.Render<FormField<string>>(parameters =>
            parameters
                .Add(p => p.Id, "field")
                .Add(p => p.Label, "Field label")
                .Add(p => p.HelpText, "Helpful hint")
                .AddChildContent("<input id=\"field\" />")
        );

        Assert.Equal("Field label", cut.Find("label").TextContent);
        Assert.Contains("Helpful hint", cut.Markup);
        Assert.NotNull(cut.Find("input#field"));
    }

    [Fact]
    public void FormTextField_renders_text_input()
    {
        var cut = context.Render<FormTextField>(parameters =>
            parameters.Add(p => p.Id, "name").Add(p => p.Label, "Name").Add(p => p.Value, "Acme")
        );

        var input = cut.Find("input#name");
        Assert.Equal("text", input.GetAttribute("type"));
        Assert.Equal("Acme", input.GetAttribute("value"));
    }

    [Fact]
    public void FormNumberField_renders_number_input()
    {
        var cut = context.Render<FormNumberField>(parameters =>
            parameters.Add(p => p.Id, "seats").Add(p => p.Label, "Seats").Add(p => p.Value, 12)
        );

        var input = cut.Find("input#seats");
        Assert.Equal("number", input.GetAttribute("type"));
        Assert.Equal("12", input.GetAttribute("value"));
    }

    [Fact]
    public void FormDecimalField_renders_decimal_input()
    {
        var cut = context.Render<FormDecimalField>(parameters =>
            parameters.Add(p => p.Id, "value").Add(p => p.Label, "Value").Add(p => p.Value, 42.5m)
        );

        var input = cut.Find("input#value");
        Assert.Equal("number", input.GetAttribute("type"));
        Assert.False(string.IsNullOrWhiteSpace(input.GetAttribute("value")));
    }

    [Fact]
    public void FormDateField_renders_date_input()
    {
        var cut = context.Render<FormDateField>(parameters =>
            parameters
                .Add(p => p.Id, "start")
                .Add(p => p.Label, "Start")
                .Add(p => p.Value, new DateOnly(2026, 1, 15))
        );

        var input = cut.Find("input#start");
        Assert.Equal("date", input.GetAttribute("type"));
        Assert.Equal("2026-01-15", input.GetAttribute("value"));
    }

    [Fact]
    public void FormTextAreaField_renders_textarea()
    {
        var cut = context.Render<FormTextAreaField>(parameters =>
            parameters
                .Add(p => p.Id, "notes")
                .Add(p => p.Label, "Notes")
                .Add(p => p.Value, "Some notes")
                .Add(p => p.Rows, 3)
        );

        var textarea = cut.Find("textarea#notes");
        Assert.Equal("3", textarea.GetAttribute("rows"));
    }

    [Fact]
    public void FormSelectEnumField_renders_all_enum_options()
    {
        var cut = context.Render<FormSelectEnumField<TestOption>>(parameters =>
            parameters
                .Add(p => p.Id, "option")
                .Add(p => p.Label, "Option")
                .Add(p => p.Value, TestOption.Unknown)
        );

        var options = cut.FindAll("select#option option");
        Assert.Equal(3, options.Count);
        Assert.Equal("Unknown", options[0].TextContent);
        Assert.Equal("Alpha", options[1].TextContent);
        Assert.Equal("Beta", options[2].TextContent);
    }

    [Fact]
    public void FormTabs_invokes_active_tab_changed()
    {
        var activeTab = 0;

        var cut = context.Render<FormTabs>(parameters =>
            parameters
                .Add(p => p.Tabs, new[] { "One", "Two", "Three" })
                .Add(p => p.ActiveTab, activeTab)
                .Add(
                    p => p.ActiveTabChanged,
                    EventCallback.Factory.Create<int>(this, index => activeTab = index)
                )
        );

        cut.FindAll("button")[1].Click();

        Assert.Equal(1, activeTab);
    }

    [Fact]
    public void FormRadioGroupField_updates_selected_value()
    {
        var selected = SingleChoiceOption.None;
        var options = new[]
        {
            new ChoiceOption<SingleChoiceOption>(SingleChoiceOption.None, "None"),
            new ChoiceOption<SingleChoiceOption>(SingleChoiceOption.Alpha, "Alpha"),
            new ChoiceOption<SingleChoiceOption>(SingleChoiceOption.Beta, "Beta"),
        };

        var cut = context.Render<FormRadioGroupField<SingleChoiceOption>>(parameters =>
            parameters
                .Add(p => p.Id, "choice")
                .Add(p => p.Label, "Choice")
                .Add(p => p.Value, selected)
                .Add(p => p.Options, options)
                .Add(
                    p => p.ValueChanged,
                    EventCallback.Factory.Create<SingleChoiceOption>(
                        this,
                        value => selected = value
                    )
                )
        );

        cut.Find("input#choice-Beta").Change(SingleChoiceOption.Beta);

        Assert.Equal(SingleChoiceOption.Beta, selected);
    }

    [Fact]
    public void FormCheckboxGroupField_toggles_selection()
    {
        var selected = new List<MultiChoiceOption>();
        var options = new[]
        {
            new ChoiceOption<MultiChoiceOption>(MultiChoiceOption.Alpha, "Alpha"),
            new ChoiceOption<MultiChoiceOption>(MultiChoiceOption.Beta, "Beta"),
        };

        var cut = context.Render<FormCheckboxGroupField<MultiChoiceOption>>(parameters =>
            parameters
                .Add(p => p.Id, "multi")
                .Add(p => p.Label, "Multi")
                .Add(p => p.Value, selected)
                .Add(p => p.Options, options)
                .Add(
                    p => p.ValueChanged,
                    EventCallback.Factory.Create<List<MultiChoiceOption>>(
                        this,
                        value => selected = value
                    )
                )
        );

        cut.Find("input#multi-Alpha").Change(true);
        Assert.Contains(MultiChoiceOption.Alpha, selected);

        cut.Find("input#multi-Alpha").Change(false);
        Assert.DoesNotContain(MultiChoiceOption.Alpha, selected);
    }

    private enum TestOption
    {
        Unknown = 0,
        Alpha = 1,
        Beta = 2,
    }
}
