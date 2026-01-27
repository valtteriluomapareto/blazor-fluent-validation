using App.Contracts;
using FluentValidation.TestHelper;

namespace App.Validation.Tests;

public sealed class CustomerIntakeFormValidatorTests
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private static CustomerIntakeFormValidator CreateValidator()
    {
        return new CustomerIntakeFormValidator();
    }

    private static CustomerIntakeForm CreateValidModel()
    {
        return new CustomerIntakeForm
        {
            CustomerName = "Acme Corp",
            ContactEmail = "name@company.com",
            SocialSecurityNumber = string.Empty,
            BusinessId = string.Empty,
            VatNumber = string.Empty,
            Seats = 25,
            EstimatedAnnualValue = 120_000,
            ExpectedStartDate = DateOnly.FromDateTime(DateTime.Today),
            ContractType = ContractType.New,
            Industry = IndustryType.SaaS,
            Notes = "Valid baseline model.",
        };
    }

    [Fact]
    public async Task Invalid_social_security_number_should_have_error()
    {
        var model = CreateValidModel();
        model.SocialSecurityNumber = "150295-1212";

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                CancellationToken
            );

        result
            .ShouldHaveValidationErrorFor(x => x.SocialSecurityNumber)
            .WithErrorCode("ssn.invalid");
    }

    [Fact]
    public async Task Valid_social_security_number_should_pass()
    {
        var model = CreateValidModel();
        model.SocialSecurityNumber = "010199-8148";

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                CancellationToken
            );

        result.ShouldNotHaveValidationErrorFor(x => x.SocialSecurityNumber);
    }

    [Fact]
    public async Task Invalid_business_id_should_have_error()
    {
        var model = CreateValidModel();
        model.BusinessId = "2617416-44";

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                CancellationToken
            );

        result.ShouldHaveValidationErrorFor(x => x.BusinessId).WithErrorCode("business_id.invalid");
    }

    [Fact]
    public async Task Valid_business_id_should_pass()
    {
        var model = CreateValidModel();
        model.BusinessId = "2617416-4";

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                CancellationToken
            );

        result.ShouldNotHaveValidationErrorFor(x => x.BusinessId);
    }

    [Fact]
    public async Task Invalid_vat_number_should_have_error()
    {
        var model = CreateValidModel();
        model.VatNumber = "FI26174164A";

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                CancellationToken
            );

        result.ShouldHaveValidationErrorFor(x => x.VatNumber).WithErrorCode("vat_number.invalid");
    }

    [Fact]
    public async Task Valid_vat_number_should_pass()
    {
        var model = CreateValidModel();
        model.VatNumber = "FI26174164";

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                CancellationToken
            );

        result.ShouldNotHaveValidationErrorFor(x => x.VatNumber);
    }

    [Fact]
    public async Task Empty_identity_fields_should_not_error()
    {
        var model = CreateValidModel();

        var result = await CreateValidator()
            .TestValidateAsync(
                model,
                options => options.IncludeRuleSets("Local"),
                CancellationToken
            );

        result.ShouldNotHaveValidationErrorFor(x => x.SocialSecurityNumber);
        result.ShouldNotHaveValidationErrorFor(x => x.BusinessId);
        result.ShouldNotHaveValidationErrorFor(x => x.VatNumber);
    }
}

