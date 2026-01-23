using App.Contracts;
using FluentValidation;

namespace App.Validation;

public sealed class SampleFormValidator : AbstractValidator<SampleForm>
{
    public SampleFormValidator()
    {
        RuleSet("Local", () =>
        {
            RuleFor(x => x.Name).NotEmpty().WithErrorCode("name.required");

            RuleFor(x => x.Age).InclusiveBetween(18, 120).WithErrorCode("age.range");
        });

        RuleSet("Server", () =>
        {
            RuleFor(x => x.Name)
                .NotEqual("Server")
                .WithErrorCode("name.server_reserved")
                .WithMessage("Name cannot be 'Server'. (SampleFormValidator)");
        });
    }
}
