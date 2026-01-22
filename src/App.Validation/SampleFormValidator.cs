using App.Contracts;
using FluentValidation;

namespace App.Validation;

public sealed class SampleFormValidator : AbstractValidator<SampleForm>
{
    public SampleFormValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode("name.required");

        RuleFor(x => x.Age)
            .InclusiveBetween(18, 120)
            .WithErrorCode("age.range");
    }
}
