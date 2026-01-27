using App.Abstractions;
using App.Contracts;
using App.Validation;
using FluentValidation;
using FormValidationTest.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddSingleton<IValidator<SampleForm>, SampleFormValidator>();
builder.Services.AddSingleton<IValidator<CustomerIntakeForm>, CustomerIntakeFormValidator>();
builder.Services.AddSingleton<
    IValidator<ValidationExamplesForm>,
    ValidationExamplesFormValidator
>();
builder.Services.AddSingleton<IUsedNameLookup, LocalUsedNameLookup>();

var configuredBaseUrl = builder.Configuration["Api:BaseUrl"];
var baseUrl = string.IsNullOrWhiteSpace(configuredBaseUrl)
    ? builder.HostEnvironment.BaseAddress
    : configuredBaseUrl;

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(baseUrl) });

await builder.Build().RunAsync();
