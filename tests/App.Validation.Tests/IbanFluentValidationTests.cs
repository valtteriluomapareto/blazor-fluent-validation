using FluentValidation;
using FluentValidation.TestHelper;
using IbanNet;
using IbanNet.FluentValidation;

namespace App.Validation.Tests;

public sealed class IbanFluentValidationTests
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Fluent_rule_should_accept_valid_iban_with_whitespace()
    {
        var validator = new TestIbanInputValidator(new IbanValidator());
        var model = new TestIbanInput { Iban = "NL91 ABNA 0417 1643 00" };

        var result = await validator.TestValidateAsync(model, cancellationToken: CancellationToken);

        result.ShouldNotHaveValidationErrorFor(x => x.Iban);
    }

    [Fact]
    public async Task Fluent_rule_should_reject_invalid_iban()
    {
        var validator = new TestIbanInputValidator(new IbanValidator());
        var model = new TestIbanInput { Iban = "NL91ABNA0417164301" };

        var result = await validator.TestValidateAsync(model, cancellationToken: CancellationToken);

        result.ShouldHaveValidationErrorFor(x => x.Iban).WithErrorCode("iban.invalid");
    }

    private sealed class TestIbanInput
    {
        public string Iban { get; init; } = string.Empty;
    }

    private sealed class TestIbanInputValidator : AbstractValidator<TestIbanInput>
    {
        public TestIbanInputValidator(IIbanValidator ibanValidator)
        {
            RuleFor(x => x.Iban)
                .NotEmpty()
                // Non-strict mode is more suitable for user input (whitespace, casing).
                .Iban(ibanValidator, strict: false)
                .WithErrorCode("iban.invalid");
        }
    }
}

