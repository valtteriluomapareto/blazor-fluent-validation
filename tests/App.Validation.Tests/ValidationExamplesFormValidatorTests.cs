using App.Contracts;
using FluentValidation.TestHelper;

namespace App.Validation.Tests;

public sealed class ValidationExamplesFormValidatorTests
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private static ValidationExamplesFormValidator CreateValidator() => new();

    private static ValidationExamplesForm CreateValidModel()
    {
        return new ValidationExamplesForm
        {
            RequiredFinnishSsn = "010199-8148",
            RequiredBusinessId = "2617416-4",
            RequiredIban = "NL91 ABNA 0417 1643 00",
            RequiredEmail = "name@example.com",
            RequiredDecimalFi = "1 234,56",
            RequiredEurAmount = "1 234,56 €",
            RequiredPercentage = "12,5%",
            RequiredSingleChoice = SingleChoiceOption.Alpha,
            RequiredMultiChoice = [MultiChoiceOption.Alpha],
        };
    }

    [Fact]
    public async Task Required_fields_should_error_when_missing()
    {
        var model = CreateValidModel();
        model.RequiredFinnishSsn = string.Empty;
        model.RequiredBusinessId = string.Empty;
        model.RequiredIban = string.Empty;
        model.RequiredEmail = string.Empty;
        model.RequiredDecimalFi = string.Empty;
        model.RequiredEurAmount = string.Empty;
        model.RequiredPercentage = string.Empty;
        model.RequiredSingleChoice = SingleChoiceOption.None;
        model.RequiredMultiChoice = [];

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                cancellationToken: CancellationToken
            );

        result
            .ShouldHaveValidationErrorFor(x => x.RequiredFinnishSsn)
            .WithErrorCode("required_finnish_ssn.required");
        result
            .ShouldHaveValidationErrorFor(x => x.RequiredBusinessId)
            .WithErrorCode("required_business_id.required");
        result
            .ShouldHaveValidationErrorFor(x => x.RequiredIban)
            .WithErrorCode("required_iban.required");
        result
            .ShouldHaveValidationErrorFor(x => x.RequiredEmail)
            .WithErrorCode("required_email.required");
        result
            .ShouldHaveValidationErrorFor(x => x.RequiredDecimalFi)
            .WithErrorCode("required_decimal_fi.required");
        result
            .ShouldHaveValidationErrorFor(x => x.RequiredEurAmount)
            .WithErrorCode("required_eur.required");
        result
            .ShouldHaveValidationErrorFor(x => x.RequiredPercentage)
            .WithErrorCode("required_percentage.required");
        result
            .ShouldHaveValidationErrorFor(x => x.RequiredSingleChoice)
            .WithErrorCode("required_single_choice.required");
        result
            .ShouldHaveValidationErrorFor(x => x.RequiredMultiChoice)
            .WithErrorCode("required_multi_choice.required");
    }

    [Fact]
    public async Task Optional_fields_should_not_error_when_empty()
    {
        var model = CreateValidModel();

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                cancellationToken: CancellationToken
            );

        result.ShouldNotHaveValidationErrorFor(x => x.OptionalFinnishSsn);
        result.ShouldNotHaveValidationErrorFor(x => x.OptionalBusinessId);
        result.ShouldNotHaveValidationErrorFor(x => x.OptionalIban);
        result.ShouldNotHaveValidationErrorFor(x => x.OptionalEmail);
        result.ShouldNotHaveValidationErrorFor(x => x.OptionalDecimalFi);
        result.ShouldNotHaveValidationErrorFor(x => x.OptionalEurAmount);
        result.ShouldNotHaveValidationErrorFor(x => x.OptionalPercentage);
        result.ShouldNotHaveValidationErrorFor(x => x.OptionalSingleChoice);
        result.ShouldNotHaveValidationErrorFor(x => x.OptionalMultiChoice);
        result.ShouldNotHaveValidationErrorFor(x => x.OptionalSingleChoiceOther);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredSingleChoiceOther);
        result.ShouldNotHaveValidationErrorFor(x => x.OptionalMultiChoiceOther);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredMultiChoiceOther);
    }

    [Fact]
    public async Task Invalid_optional_values_should_have_errors()
    {
        var model = CreateValidModel();
        model.OptionalFinnishSsn = "150295-1212";
        model.OptionalBusinessId = "2617416-44";
        model.OptionalIban = "NL91ABNA0417164301";
        model.OptionalEmail = "not-an-email";
        model.OptionalDecimalFi = "not-a-number";
        model.OptionalEurAmount = "not-a-number €";
        model.OptionalPercentage = "150%";

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                cancellationToken: CancellationToken
            );

        result
            .ShouldHaveValidationErrorFor(x => x.OptionalFinnishSsn)
            .WithErrorCode("optional_finnish_ssn.invalid");
        result
            .ShouldHaveValidationErrorFor(x => x.OptionalBusinessId)
            .WithErrorCode("optional_business_id.invalid");
        result
            .ShouldHaveValidationErrorFor(x => x.OptionalIban)
            .WithErrorCode("optional_iban.invalid");
        result
            .ShouldHaveValidationErrorFor(x => x.OptionalEmail)
            .WithErrorCode("optional_email.invalid");
        result
            .ShouldHaveValidationErrorFor(x => x.OptionalDecimalFi)
            .WithErrorCode("optional_decimal_fi.invalid");
        result
            .ShouldHaveValidationErrorFor(x => x.OptionalEurAmount)
            .WithErrorCode("optional_eur.invalid");
        result
            .ShouldHaveValidationErrorFor(x => x.OptionalPercentage)
            .WithErrorCode("optional_percentage.range");
    }

    [Fact]
    public async Task Valid_required_values_should_pass()
    {
        var model = CreateValidModel();

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                cancellationToken: CancellationToken
            );

        result.ShouldNotHaveValidationErrorFor(x => x.RequiredFinnishSsn);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredBusinessId);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredIban);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredEmail);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredDecimalFi);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredEurAmount);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredPercentage);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredSingleChoice);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredMultiChoice);
    }

    [Fact]
    public async Task Other_values_should_be_required_when_other_selected()
    {
        var model = CreateValidModel();
        model.OptionalSingleChoice = SingleChoiceOption.Other;
        model.RequiredSingleChoice = SingleChoiceOption.Other;
        model.OptionalMultiChoice = [MultiChoiceOption.Other];
        model.RequiredMultiChoice = [MultiChoiceOption.Other];

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                cancellationToken: CancellationToken
            );

        result
            .ShouldHaveValidationErrorFor(x => x.OptionalSingleChoiceOther)
            .WithErrorCode("optional_single_choice.other_required");
        result
            .ShouldHaveValidationErrorFor(x => x.RequiredSingleChoiceOther)
            .WithErrorCode("required_single_choice.other_required");
        result
            .ShouldHaveValidationErrorFor(x => x.OptionalMultiChoiceOther)
            .WithErrorCode("optional_multi_choice.other_required");
        result
            .ShouldHaveValidationErrorFor(x => x.RequiredMultiChoiceOther)
            .WithErrorCode("required_multi_choice.other_required");
    }

    [Fact]
    public async Task Other_values_should_pass_when_other_selected_and_text_provided()
    {
        var model = CreateValidModel();
        model.OptionalSingleChoice = SingleChoiceOption.Other;
        model.OptionalSingleChoiceOther = "Delta";
        model.RequiredSingleChoice = SingleChoiceOption.Other;
        model.RequiredSingleChoiceOther = "Epsilon";
        model.OptionalMultiChoice = [MultiChoiceOption.Other];
        model.OptionalMultiChoiceOther = "Zeta";
        model.RequiredMultiChoice = [MultiChoiceOption.Other];
        model.RequiredMultiChoiceOther = "Eta";

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                cancellationToken: CancellationToken
            );

        result.ShouldNotHaveValidationErrorFor(x => x.OptionalSingleChoiceOther);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredSingleChoiceOther);
        result.ShouldNotHaveValidationErrorFor(x => x.OptionalMultiChoiceOther);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredMultiChoiceOther);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredSingleChoice);
        result.ShouldNotHaveValidationErrorFor(x => x.RequiredMultiChoice);
    }
}
