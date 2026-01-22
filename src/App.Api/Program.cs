using App.Contracts;
using App.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IValidator<SampleForm>, SampleFormValidator>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost(
    "/api/sample-form",
    async (SampleForm model, IValidator<SampleForm> validator) =>
    {
        var result = await validator.ValidateAsync(
            model,
            options => options.IncludeRuleSets("Local", "Server")
        );
        if (!result.IsValid)
        {
            return Results.BadRequest(
                CreateValidationProblemDetails(
                    result.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(error => error.ErrorMessage).ToArray()
                        ),
                    result.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(error => error.ErrorCode).ToArray()
                        )
                )
            );
        }

        if (string.Equals(model.Name, "ApiOnly", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(
                CreateValidationProblemDetails(
                    new Dictionary<string, string[]>
                    {
                        ["Name"] = ["Name cannot be 'ApiOnly'."]
                    },
                    new Dictionary<string, string[]>
                    {
                        ["Name"] = ["name.api_reserved"]
                    }
                )
            );
        }

        return Results.Ok(new SampleFormResponse("Form is valid."));
    }
);

static ValidationProblemDetails CreateValidationProblemDetails(
    Dictionary<string, string[]> errors,
    Dictionary<string, string[]> errorCodes
)
{
    var problemDetails = new ValidationProblemDetails(errors)
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "Validation failed."
    };
    problemDetails.Extensions["errorCodes"] = errorCodes;

    return problemDetails;
}

app.Run();
