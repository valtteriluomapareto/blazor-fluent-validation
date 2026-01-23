using App.Abstractions;
using App.Api.Validation;
using App.Contracts;
using App.Integrations;
using App.Validation;
using FluentValidation;

namespace App.Api;

public static class ApiModule
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<SampleForm>, SampleFormValidator>();
        services.AddSingleton<IUsedNameLookup, MockUsedNameLookup>();

        return services;
    }

    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
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

        return app;
    }
}
