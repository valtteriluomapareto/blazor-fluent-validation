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
        services.AddSingleton<
            IValidator<ValidationExamplesForm>,
            ValidationExamplesFormValidator
        >();
        services.AddSingleton<
            IValidator<PrefillIntegrationDemoForm>,
            PrefillIntegrationDemoFormValidator
        >();
        services.AddSingleton<IUsedNameLookup, MockUsedNameLookup>();
        services.AddSingleton<IPrefillIntegrationLookup, MockPrefillIntegrationLookup>();

        return services;
    }

    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/sample-form",
                (HttpContext httpContext, SampleForm model) =>
                {
                    if (string.Equals(model.Name, "ApiOnly", StringComparison.OrdinalIgnoreCase))
                    {
                        var errors = new Dictionary<string, string[]>
                        {
                            ["Name"] =
                            [
                                "Name cannot be 'ApiOnly'. (POST /api/sample-form endpoint)",
                            ],
                        };

                        var errorCodes = new Dictionary<string, string[]>
                        {
                            ["Name"] = ["name.api_reserved"],
                        };

                        return Results.BadRequest(
                            ValidationErrorResponseFactory.Create(
                                httpContext,
                                errors,
                                errorCodes,
                                detail: "Endpoint-only validation failed."
                            )
                        );
                    }

                    return Results.Ok(new SampleFormResponse("Form is valid."));
                }
            )
            .AddEndpointFilter(new ValidationFilter<SampleForm>("Local", "Server"));

        app.MapGet(
            "/api/prefill-integration-demo",
            async (
                string? name,
                IPrefillIntegrationLookup prefillLookup,
                CancellationToken cancellationToken
            ) =>
            {
                var lookupName = name?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(lookupName))
                {
                    return Results.Ok(
                        new PrefillIntegrationDemoLookupResponse(
                            Found: false,
                            LookupName: lookupName,
                            MatchingName: PrefillIntegrationDemoDefaults.MatchingName,
                            Data: null,
                            Message: "Enter a name to look up existing data."
                        )
                    );
                }

                var data = await prefillLookup.LookupAsync(lookupName, cancellationToken);
                var found = data is not null;
                var message = found
                    ? "Integration returned existing data."
                    : "No integration data found for that name.";

                return Results.Ok(
                    new PrefillIntegrationDemoLookupResponse(
                        Found: found,
                        LookupName: lookupName,
                        MatchingName: PrefillIntegrationDemoDefaults.MatchingName,
                        Data: data,
                        Message: message
                    )
                );
            }
        );

        return app;
    }
}
