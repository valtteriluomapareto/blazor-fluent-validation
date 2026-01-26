using App.Contracts;
using FluentValidation;

namespace App.Validation;

public sealed class CustomerIntakeFormValidator : AbstractValidator<CustomerIntakeForm>
{
    public CustomerIntakeFormValidator()
    {
        RuleSet("Local", () =>
        {
            RuleFor(x => x.CustomerName)
                .NotEmpty()
                .WithErrorCode("customer_name.required")
                .MaximumLength(120)
                .WithErrorCode("customer_name.length");

            RuleFor(x => x.ContactEmail)
                .NotEmpty()
                .WithErrorCode("contact_email.required")
                .EmailAddress()
                .WithErrorCode("contact_email.invalid");

            RuleFor(x => x.Seats)
                .InclusiveBetween(1, 5000)
                .WithErrorCode("seats.range");

            RuleFor(x => x.EstimatedAnnualValue)
                .InclusiveBetween(0, 10_000_000)
                .WithErrorCode("arr.range");

            RuleFor(x => x.ExpectedStartDate)
                .Must(date => date >= DateOnly.FromDateTime(DateTime.Today))
                .WithErrorCode("start_date.future");

            RuleFor(x => x.ContractType)
                .NotEqual(ContractType.Unknown)
                .WithErrorCode("contract_type.required");

            RuleFor(x => x.Industry)
                .NotEqual(IndustryType.Unknown)
                .WithErrorCode("industry.required");

            RuleFor(x => x.Notes)
                .MaximumLength(500)
                .WithErrorCode("notes.length");
        });
    }
}
