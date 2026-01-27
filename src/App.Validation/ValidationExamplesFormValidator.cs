using App.Contracts;
using FluentValidation;

namespace App.Validation;

public sealed class ValidationExamplesFormValidator : AbstractValidator<ValidationExamplesForm>
{
    private const decimal MaxEurAmount = 10_000_000m;

    public ValidationExamplesFormValidator()
    {
        RuleSet(
            "Local",
            () =>
            {
                AddFinnishSsnRules();
                AddBusinessIdRules();
                AddIbanRules();
                AddEmailRules();
                AddDecimalRules();
                AddCurrencyRules();
                AddPercentageRules();
                AddEnumSelectRules();
                AddChoiceRules();
            }
        );
    }

    private void AddFinnishSsnRules()
    {
        RuleFor(x => x.OptionalFinnishSsn)
            .Must(FinnishSsn.Validate)
            .WithErrorCode("optional_finnish_ssn.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.OptionalFinnishSsn));

        RuleFor(x => x.RequiredFinnishSsn)
            .NotEmpty()
            .WithErrorCode("required_finnish_ssn.required");

        RuleFor(x => x.RequiredFinnishSsn)
            .Must(FinnishSsn.Validate)
            .WithErrorCode("required_finnish_ssn.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.RequiredFinnishSsn));
    }

    private void AddBusinessIdRules()
    {
        RuleFor(x => x.OptionalBusinessId)
            .Must(FinnishBusinessIds.IsValidBusinessId)
            .WithErrorCode("optional_business_id.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.OptionalBusinessId));

        RuleFor(x => x.RequiredBusinessId)
            .NotEmpty()
            .WithErrorCode("required_business_id.required");

        RuleFor(x => x.RequiredBusinessId)
            .Must(FinnishBusinessIds.IsValidBusinessId)
            .WithErrorCode("required_business_id.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.RequiredBusinessId));
    }

    private void AddIbanRules()
    {
        RuleFor(x => x.OptionalIban)
            .Must(IbanValidation.IsValid)
            .WithErrorCode("optional_iban.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.OptionalIban));

        RuleFor(x => x.RequiredIban).NotEmpty().WithErrorCode("required_iban.required");

        RuleFor(x => x.RequiredIban)
            .Must(IbanValidation.IsValid)
            .WithErrorCode("required_iban.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.RequiredIban));
    }

    private void AddEmailRules()
    {
        RuleFor(x => x.OptionalEmail)
            .EmailAddress()
            .WithErrorCode("optional_email.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.OptionalEmail));

        RuleFor(x => x.RequiredEmail).NotEmpty().WithErrorCode("required_email.required");

        RuleFor(x => x.RequiredEmail)
            .EmailAddress()
            .WithErrorCode("required_email.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.RequiredEmail));
    }

    private void AddDecimalRules()
    {
        RuleFor(x => x.OptionalDecimalFi)
            .Must(value => FinnishNumberParsing.TryParseDecimal(value, out _))
            .WithErrorCode("optional_decimal_fi.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.OptionalDecimalFi));

        RuleFor(x => x.RequiredDecimalFi).NotEmpty().WithErrorCode("required_decimal_fi.required");

        RuleFor(x => x.RequiredDecimalFi)
            .Must(value => FinnishNumberParsing.TryParseDecimal(value, out _))
            .WithErrorCode("required_decimal_fi.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.RequiredDecimalFi));
    }

    private void AddCurrencyRules()
    {
        RuleFor(x => x.OptionalEurAmount)
            .Must(value => FinnishNumberParsing.TryParseCurrencyEur(value, out _))
            .WithErrorCode("optional_eur.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.OptionalEurAmount));

        RuleFor(x => x.OptionalEurAmount)
            .Must(value =>
                FinnishNumberParsing.TryParseCurrencyEur(value, out var amount)
                && amount >= 0m
                && amount <= MaxEurAmount
            )
            .WithErrorCode("optional_eur.range")
            .When(x =>
                !string.IsNullOrWhiteSpace(x.OptionalEurAmount)
                && FinnishNumberParsing.TryParseCurrencyEur(x.OptionalEurAmount, out _)
            );

        RuleFor(x => x.RequiredEurAmount).NotEmpty().WithErrorCode("required_eur.required");

        RuleFor(x => x.RequiredEurAmount)
            .Must(value => FinnishNumberParsing.TryParseCurrencyEur(value, out _))
            .WithErrorCode("required_eur.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.RequiredEurAmount));

        RuleFor(x => x.RequiredEurAmount)
            .Must(value =>
                FinnishNumberParsing.TryParseCurrencyEur(value, out var amount)
                && amount >= 0m
                && amount <= MaxEurAmount
            )
            .WithErrorCode("required_eur.range")
            .When(x =>
                !string.IsNullOrWhiteSpace(x.RequiredEurAmount)
                && FinnishNumberParsing.TryParseCurrencyEur(x.RequiredEurAmount, out _)
            );
    }

    private void AddPercentageRules()
    {
        RuleFor(x => x.OptionalPercentage)
            .Must(value => FinnishNumberParsing.TryParsePercentage(value, out _))
            .WithErrorCode("optional_percentage.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.OptionalPercentage));

        RuleFor(x => x.OptionalPercentage)
            .Must(value =>
                FinnishNumberParsing.TryParsePercentage(value, out var percentage)
                && percentage >= 0m
                && percentage <= 100m
            )
            .WithErrorCode("optional_percentage.range")
            .When(x =>
                !string.IsNullOrWhiteSpace(x.OptionalPercentage)
                && FinnishNumberParsing.TryParsePercentage(x.OptionalPercentage, out _)
            );

        RuleFor(x => x.RequiredPercentage).NotEmpty().WithErrorCode("required_percentage.required");

        RuleFor(x => x.RequiredPercentage)
            .Must(value => FinnishNumberParsing.TryParsePercentage(value, out _))
            .WithErrorCode("required_percentage.invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.RequiredPercentage));

        RuleFor(x => x.RequiredPercentage)
            .Must(value =>
                FinnishNumberParsing.TryParsePercentage(value, out var percentage)
                && percentage >= 0m
                && percentage <= 100m
            )
            .WithErrorCode("required_percentage.range")
            .When(x =>
                !string.IsNullOrWhiteSpace(x.RequiredPercentage)
                && FinnishNumberParsing.TryParsePercentage(x.RequiredPercentage, out _)
            );
    }

    private void AddEnumSelectRules()
    {
        RuleFor(x => x.SentinelIndustry)
            .NotEqual(IndustryType.Unknown)
            .WithErrorCode("sentinel_industry.required");

        RuleFor(x => x.NullableIndustry).NotNull().WithErrorCode("nullable_industry.required");
    }

    private void AddChoiceRules()
    {
        RuleFor(x => x.RequiredSingleChoice)
            .Must(choice => choice != SingleChoiceOption.None)
            .WithErrorCode("required_single_choice.required");

        RuleFor(x => x.RequiredMultiChoice)
            .Must(choices => choices.Count > 0)
            .WithErrorCode("required_multi_choice.required");

        RuleFor(x => x.OptionalSingleChoiceOther)
            .NotEmpty()
            .WithErrorCode("optional_single_choice.other_required")
            .When(x => x.OptionalSingleChoice == SingleChoiceOption.Other);

        RuleFor(x => x.RequiredSingleChoiceOther)
            .NotEmpty()
            .WithErrorCode("required_single_choice.other_required")
            .When(x => x.RequiredSingleChoice == SingleChoiceOption.Other);

        RuleFor(x => x.OptionalMultiChoiceOther)
            .NotEmpty()
            .WithErrorCode("optional_multi_choice.other_required")
            .When(x => x.OptionalMultiChoice.Contains(MultiChoiceOption.Other));

        RuleFor(x => x.RequiredMultiChoiceOther)
            .NotEmpty()
            .WithErrorCode("required_multi_choice.other_required")
            .When(x => x.RequiredMultiChoice.Contains(MultiChoiceOption.Other));
    }
}
