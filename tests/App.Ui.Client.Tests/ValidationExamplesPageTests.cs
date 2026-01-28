using App.Contracts;
using App.Validation;
using FluentValidation;
using FormValidationTest.Client.Pages;
using FormValidationTest.Client.Services.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace App.Ui.Client.Tests;

public sealed class ValidationExamplesPageTests : IDisposable
{
    private readonly BunitContext _context = new();

    public ValidationExamplesPageTests()
    {
        _context.Services.AddSingleton<
            IValidator<ValidationExamplesForm>,
            ValidationExamplesFormValidator
        >();
        _context.Services.AddSingleton<IValidationMessageLocalizer, ValidationMessageLocalizer>();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public void ValidationExamples_renders_all_requested_input_types()
    {
        var cut = _context.Render<ValidationExamples>();

        Assert.NotNull(cut.Find("input#optional-ssn"));
        Assert.NotNull(cut.Find("input#required-ssn"));

        Assert.NotNull(cut.Find("input#optional-business-id"));
        Assert.NotNull(cut.Find("input#required-business-id"));

        Assert.NotNull(cut.Find("input#optional-iban"));
        Assert.NotNull(cut.Find("input#required-iban"));

        Assert.NotNull(cut.Find("input#optional-email"));
        Assert.NotNull(cut.Find("input#required-email"));

        Assert.NotNull(cut.Find("input#optional-decimal-fi"));
        Assert.NotNull(cut.Find("input#required-decimal-fi"));

        Assert.NotNull(cut.Find("input#optional-eur"));
        Assert.NotNull(cut.Find("input#required-eur"));

        Assert.NotNull(cut.Find("input#optional-percentage"));
        Assert.NotNull(cut.Find("input#required-percentage"));

        Assert.NotNull(cut.Find("select#sentinel-industry"));
        Assert.NotNull(cut.Find("select#nullable-industry"));

        var sentinelOptions = cut.FindAll("select#sentinel-industry option");
        Assert.Equal("Select an industry", sentinelOptions[0].TextContent);

        Assert.NotNull(
            cut.Find("input[id^='optional-single-choice-opt-'][data-option-value='Alpha']")
        );
        Assert.NotNull(
            cut.Find("input[id^='required-single-choice-opt-'][data-option-value='Alpha']")
        );
        Assert.NotNull(
            cut.Find("input[id^='optional-single-choice-opt-'][data-option-value='Other']")
        );
        Assert.NotNull(
            cut.Find("input[id^='required-single-choice-opt-'][data-option-value='Other']")
        );

        Assert.NotNull(
            cut.Find("input[id^='optional-multi-choice-opt-'][data-option-value='Alpha']")
        );
        Assert.NotNull(
            cut.Find("input[id^='required-multi-choice-opt-'][data-option-value='Alpha']")
        );
        Assert.NotNull(
            cut.Find("input[id^='optional-multi-choice-opt-'][data-option-value='Other']")
        );
        Assert.NotNull(
            cut.Find("input[id^='required-multi-choice-opt-'][data-option-value='Other']")
        );

        Assert.Empty(cut.FindAll("input#optional-single-choice-other-value"));
        Assert.Empty(cut.FindAll("input#required-single-choice-other-value"));
        Assert.Empty(cut.FindAll("input#optional-multi-choice-other-value"));
        Assert.Empty(cut.FindAll("input#required-multi-choice-other-value"));
    }
}
