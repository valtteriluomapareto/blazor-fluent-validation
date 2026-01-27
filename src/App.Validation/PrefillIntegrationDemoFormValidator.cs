using App.Contracts;
using FluentValidation;

namespace App.Validation;

public sealed class PrefillIntegrationDemoFormValidator
    : AbstractValidator<PrefillIntegrationDemoForm>
{
    private const string PhonePattern = "^[0-9+()\\-\\s]{7,30}$";

    public PrefillIntegrationDemoFormValidator()
    {
        RuleSet(
            "Local",
            () =>
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithErrorCode("name.required")
                    .MaximumLength(120)
                    .WithErrorCode("name.length");

                RuleFor(x => x.AddressLine1)
                    .NotEmpty()
                    .WithErrorCode("address_line1.required")
                    .MaximumLength(200)
                    .WithErrorCode("address_line1.length");

                RuleFor(x => x.AddressLine2)
                    .MaximumLength(200)
                    .WithErrorCode("address_line2.length");

                RuleFor(x => x.City)
                    .NotEmpty()
                    .WithErrorCode("city.required")
                    .MaximumLength(120)
                    .WithErrorCode("city.length");

                RuleFor(x => x.PostalCode)
                    .NotEmpty()
                    .WithErrorCode("postal_code.required")
                    .MaximumLength(20)
                    .WithErrorCode("postal_code.length");

                RuleFor(x => x.PhoneNumber)
                    .NotEmpty()
                    .WithErrorCode("phone.required")
                    .Matches(PhonePattern)
                    .WithErrorCode("phone.invalid");

                RuleFor(x => x.Email)
                    .NotEmpty()
                    .WithErrorCode("email.required")
                    .EmailAddress()
                    .WithErrorCode("email.invalid");
            }
        );
    }
}
