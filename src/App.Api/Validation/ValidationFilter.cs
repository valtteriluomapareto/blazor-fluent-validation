using App.Contracts;
using FluentValidation;

namespace App.Api.Validation;

public sealed class ValidationFilter<T> : IEndpointFilter
    where T : class
{
    private readonly string[] ruleSets;

    public ValidationFilter(params string[] ruleSets)
    {
        this.ruleSets = ruleSets.Length == 0 ? ["Local", "Server"] : ruleSets;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null)
        {
            return await next(context);
        }

        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
        {
            return await next(context);
        }

        var result = await validator.ValidateAsync(
            model,
            options =>
            {
                if (ruleSets.Length > 0)
                {
                    options.IncludeRuleSets(ruleSets);
                }
            }
        );

        if (result.IsValid)
        {
            return await next(context);
        }

        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray()
            );

        var errorCodes = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorCode).ToArray()
            );

        return Results.BadRequest(
            new ValidationErrorResponse("Validation failed.", StatusCodes.Status400BadRequest, errors, errorCodes)
        );
    }
}
