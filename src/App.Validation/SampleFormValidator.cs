using App.Abstractions;
using App.Contracts;
using FluentValidation;

namespace App.Validation;

public sealed class SampleFormValidator : AbstractValidator<SampleForm>
{
    private readonly IUsedNameLookup usedNameLookup;

    public SampleFormValidator(IUsedNameLookup usedNameLookup)
    {
        this.usedNameLookup = usedNameLookup;

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

            RuleFor(x => x.Name)
                .MustAsync(async (name, cancellationToken) =>
                {
                    var usedNames = await usedNameLookup.GetUsedNamesAsync(cancellationToken);
                    return !usedNames.Contains(name, StringComparer.OrdinalIgnoreCase);
                })
                .WithErrorCode("name.already_used")
                .WithMessage("Name is already used.")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));
        });
    }
}
