using App.Contracts;
using App.Validation;
using FluentValidation;
using FormValidationTest.Client.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace App.Ui.Client.Tests;

public sealed class ValidationExamplesPageTests : IDisposable
{
    private readonly BunitContext context = new();

    public ValidationExamplesPageTests()
    {
        context.Services.AddSingleton<
            IValidator<ValidationExamplesForm>,
            ValidationExamplesFormValidator
        >();
    }

    public void Dispose() => context.Dispose();

    [Fact]
    public void ValidationExamples_renders_all_requested_input_types()
    {
        var cut = context.Render<ValidationExamples>();

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

        Assert.NotNull(cut.Find("input#optional-single-choice-Alpha"));
        Assert.NotNull(cut.Find("input#required-single-choice-Alpha"));
        Assert.NotNull(cut.Find("input#optional-single-choice-Other"));
        Assert.NotNull(cut.Find("input#required-single-choice-Other"));

        Assert.NotNull(cut.Find("input#optional-multi-choice-Alpha"));
        Assert.NotNull(cut.Find("input#required-multi-choice-Alpha"));
        Assert.NotNull(cut.Find("input#optional-multi-choice-Other"));
        Assert.NotNull(cut.Find("input#required-multi-choice-Other"));

        Assert.Empty(cut.FindAll("input#optional-single-choice-other-value"));
        Assert.Empty(cut.FindAll("input#required-single-choice-other-value"));
        Assert.Empty(cut.FindAll("input#optional-multi-choice-other-value"));
        Assert.Empty(cut.FindAll("input#required-multi-choice-other-value"));
    }
}
