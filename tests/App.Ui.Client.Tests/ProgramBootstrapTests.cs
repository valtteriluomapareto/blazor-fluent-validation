using System.Globalization;
using App.Abstractions;
using App.Contracts;
using App.Validation;
using FluentValidation;
using FormValidationTest.Client;
using FormValidationTest.Client.Services.Forms;
using FormValidationTest.Client.Services.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Ui.Client.Tests;

public sealed class ProgramBootstrapTests
{
    [Fact]
    public void ConfigureCulture_sets_default_culture_to_finnish()
    {
        var previousCulture = CultureInfo.DefaultThreadCurrentCulture;
        var previousUiCulture = CultureInfo.DefaultThreadCurrentUICulture;

        try
        {
            ClientProgram.ConfigureCulture();

            Assert.Equal("fi-FI", CultureInfo.DefaultThreadCurrentCulture?.Name);
            Assert.Equal("fi-FI", CultureInfo.DefaultThreadCurrentUICulture?.Name);
        }
        finally
        {
            CultureInfo.DefaultThreadCurrentCulture = previousCulture;
            CultureInfo.DefaultThreadCurrentUICulture = previousUiCulture;
        }
    }

    [Fact]
    public void RegisterServices_registers_core_services()
    {
        var services = new ServiceCollection();
        ClientProgram.RegisterServices(services);
        using var provider = services.BuildServiceProvider();

        Assert.IsType<CustomerIntakeFormValidator>(
            provider.GetRequiredService<IValidator<CustomerIntakeForm>>()
        );
        Assert.IsType<SampleFormValidator>(provider.GetRequiredService<IValidator<SampleForm>>());
        Assert.IsType<ValidationExamplesFormValidator>(
            provider.GetRequiredService<IValidator<ValidationExamplesForm>>()
        );
        Assert.IsType<PrefillIntegrationDemoFormValidator>(
            provider.GetRequiredService<IValidator<PrefillIntegrationDemoForm>>()
        );

        Assert.NotNull(provider.GetRequiredService<IUsedNameLookup>());
        Assert.IsType<ValidationMessageLocalizer>(
            provider.GetRequiredService<IValidationMessageLocalizer>()
        );
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IApiFormSubmitter)
        );
    }

    [Fact]
    public void ResolveBaseUrl_uses_host_base_address_when_configuration_is_missing()
    {
        const string hostBaseAddress = "https://host.example.test/";
        var configuration = new ConfigurationBuilder().Build();

        var baseUrl = ClientProgram.ResolveBaseUrl(configuration, hostBaseAddress);

        Assert.Equal(hostBaseAddress, baseUrl);
    }

    [Fact]
    public void ResolveBaseUrl_uses_configured_api_base_url_when_provided()
    {
        const string hostBaseAddress = "https://host.example.test/";
        const string configuredBaseUrl = "https://api.example.test/";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["Api:BaseUrl"] = configuredBaseUrl }
            )
            .Build();

        var baseUrl = ClientProgram.ResolveBaseUrl(configuration, hostBaseAddress);

        Assert.Equal(configuredBaseUrl, baseUrl);
    }
}
