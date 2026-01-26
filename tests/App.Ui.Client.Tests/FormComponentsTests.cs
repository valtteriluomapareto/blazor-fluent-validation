using FormValidationTest.Client.Components.Forms;

namespace App.Ui.Client.Tests;

public sealed class FormComponentsTests : TestContext
{
    [Fact]
    public void FormCard_renders_child_content()
    {
        var cut = RenderComponent<FormCard>(parameters =>
            parameters.AddChildContent("<p>Card content</p>")
        );

        Assert.Contains("Card content", cut.Markup);
        Assert.Contains("rounded-xl", cut.Markup);
    }

    [Fact]
    public void FormField_renders_label_and_help_text()
    {
        var cut = RenderComponent<FormField<string>>(parameters =>
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
        var cut = RenderComponent<FormTextField>(parameters =>
            parameters
                .Add(p => p.Id, "name")
                .Add(p => p.Label, "Name")
                .Add(p => p.Value, "Acme")
        );

        var input = cut.Find("input#name");
        Assert.Equal("text", input.GetAttribute("type"));
        Assert.Equal("Acme", input.GetAttribute("value"));
    }

    [Fact]
    public void FormNumberField_renders_number_input()
    {
        var cut = RenderComponent<FormNumberField>(parameters =>
            parameters
                .Add(p => p.Id, "seats")
                .Add(p => p.Label, "Seats")
                .Add(p => p.Value, 12)
        );

        var input = cut.Find("input#seats");
        Assert.Equal("number", input.GetAttribute("type"));
        Assert.Equal("12", input.GetAttribute("value"));
    }

    [Fact]
    public void FormDecimalField_renders_decimal_input()
    {
        var cut = RenderComponent<FormDecimalField>(parameters =>
            parameters
                .Add(p => p.Id, "value")
                .Add(p => p.Label, "Value")
                .Add(p => p.Value, 42.5m)
        );

        var input = cut.Find("input#value");
        Assert.Equal("number", input.GetAttribute("type"));
        Assert.False(string.IsNullOrWhiteSpace(input.GetAttribute("value")));
    }

    [Fact]
    public void FormDateField_renders_date_input()
    {
        var cut = RenderComponent<FormDateField>(parameters =>
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
        var cut = RenderComponent<FormTextAreaField>(parameters =>
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
        var cut = RenderComponent<FormSelectEnumField<TestOption>>(parameters =>
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

    private enum TestOption
    {
        Unknown = 0,
        Alpha = 1,
        Beta = 2
    }
}
