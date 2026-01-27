using System.Globalization;
using App.Abstractions;
using App.Contracts;
using App.Validation;
using FluentValidation;
using FormValidationTest.Client.Services.Validation;
using FormValidationTest.Services;

var builder = WebApplication.CreateBuilder(args);

var fiCulture = CultureInfo.GetCultureInfo("fi-FI");
CultureInfo.DefaultThreadCurrentCulture = fiCulture;
CultureInfo.DefaultThreadCurrentUICulture = fiCulture;

// Add services to the container.
builder
    .Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddSingleton<IValidator<SampleForm>, SampleFormValidator>();
builder.Services.AddSingleton<IValidator<CustomerIntakeForm>, CustomerIntakeFormValidator>();
builder.Services.AddSingleton<
    IValidator<PrefillIntegrationDemoForm>,
    PrefillIntegrationDemoFormValidator
>();
builder.Services.AddSingleton<IUsedNameLookup, LocalUsedNameLookup>();
builder.Services.AddSingleton<IValidationMessageLocalizer, ValidationMessageLocalizer>();
builder.Services.AddHttpClient(
    "Api",
    client =>
    {
        var baseUrl = builder.Configuration["Api:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "http://localhost:5113";
        }

        client.BaseAddress = new Uri(baseUrl);
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<FormValidationTest.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FormValidationTest.Client._Imports).Assembly);

app.Run();
