using App.Api.Validation;
using App.Contracts;
using App.Validation;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IValidator<SampleForm>, SampleFormValidator>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost(
    "/api/sample-form",
    (SampleForm model) =>
    {
        if (string.Equals(model.Name, "ApiOnly", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(
                new ValidationErrorResponse(
                    "Validation failed.",
                    StatusCodes.Status400BadRequest,
                    new Dictionary<string, string[]>
                    {
                        ["Name"] = ["Name cannot be 'ApiOnly'. (POST /api/sample-form endpoint)"]
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
).AddEndpointFilter(new ValidationFilter<SampleForm>("Local", "Server"));

app.Run();

public partial class Program;
