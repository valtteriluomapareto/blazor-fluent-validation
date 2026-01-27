using App.Contracts;
using FluentValidation.TestHelper;

namespace App.Validation.Tests;

public sealed class PrefillIntegrationDemoFormValidatorTests
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private static PrefillIntegrationDemoFormValidator CreateValidator()
    {
        return new PrefillIntegrationDemoFormValidator();
    }

    private static PrefillIntegrationDemoForm CreateValidModel()
    {
        return new PrefillIntegrationDemoForm
        {
            Name = PrefillIntegrationDemoDefaults.MatchingName,
            AddressLine1 = "123 Analytical Engine Way",
            AddressLine2 = "Suite 42",
            City = "London",
            PostalCode = "SW1A 1AA",
            PhoneNumber = "+44 20 7946 0958",
            Email = "ada.lovelace@example.com",
        };
    }

    [Fact]
    public async Task Valid_model_should_pass_local_rules()
    {
        var model = CreateValidModel();

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                CancellationToken
            );

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Empty_name_should_have_required_error()
    {
        var model = CreateValidModel();
        model.Name = string.Empty;

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                CancellationToken
            );

        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorCode("name.required");
    }

    [Fact]
    public async Task Invalid_phone_should_have_error()
    {
        var model = CreateValidModel();
        model.PhoneNumber = "abc";

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                CancellationToken
            );

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber).WithErrorCode("phone.invalid");
    }

    [Fact]
    public async Task Invalid_email_should_have_error()
    {
        var model = CreateValidModel();
        model.Email = "not-an-email";

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                CancellationToken
            );

        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorCode("email.invalid");
    }
}
