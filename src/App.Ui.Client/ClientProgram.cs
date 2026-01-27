using System.Globalization;
using App.Abstractions;
using App.Contracts;
using App.Validation;
using FluentValidation;
using FormValidationTest.Client.Services;
using FormValidationTest.Client.Services.Validation;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FormValidationTest.Client;

public static class ClientProgram
{
    public static void ConfigureCulture(string cultureName = "fi-FI")
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IValidator<SampleForm>, SampleFormValidator>();
        services.AddSingleton<IValidator<CustomerIntakeForm>, CustomerIntakeFormValidator>();
        services.AddSingleton<
            IValidator<ValidationExamplesForm>,
            ValidationExamplesFormValidator
        >();
        services.AddSingleton<
            IValidator<PrefillIntegrationDemoForm>,
            PrefillIntegrationDemoFormValidator
        >();
        services.AddSingleton<IUsedNameLookup, LocalUsedNameLookup>();
        services.AddSingleton<IValidationMessageLocalizer, ValidationMessageLocalizer>();
    }

    public static string ResolveBaseUrl(IConfiguration configuration, string hostBaseAddress)
    {
        var configuredBaseUrl = configuration["Api:BaseUrl"];
        return string.IsNullOrWhiteSpace(configuredBaseUrl) ? hostBaseAddress : configuredBaseUrl;
    }

    public static WebAssemblyHostBuilder CreateBuilder(
        string[]? args = null,
        IReadOnlyDictionary<string, string?>? configurationOverrides = null
    )
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args ?? Array.Empty<string>());

        if (configurationOverrides is not null)
        {
            builder.Configuration.AddInMemoryCollection(configurationOverrides);
        }

        ConfigureCulture();

        RegisterServices(builder.Services);

        var baseUrl = ResolveBaseUrl(builder.Configuration, builder.HostEnvironment.BaseAddress);

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(baseUrl) });

        return builder;
    }

    public static async Task RunAsync(string[] args)
    {
        var builder = CreateBuilder(args);
        await builder.Build().RunAsync();
    }
}
