using System.Globalization;
using App.Api;
using App.Contracts;
using App.Validation;
using FluentValidation;
using FormValidationTest.Client.Services.Http;
using FormValidationTest.Client.Services.Validation;

var builder = WebApplication.CreateBuilder(args);

var fiCulture = CultureInfo.GetCultureInfo("fi-FI");
CultureInfo.DefaultThreadCurrentCulture = fiCulture;
CultureInfo.DefaultThreadCurrentUICulture = fiCulture;

builder.WebHost.UseStaticWebAssets();
builder
    .Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddTransient<ResilientHttpMessageHandler>();
builder
    .Services.AddHttpClient(
        "Api",
        client => client.Timeout = ResilientHttpMessageHandler.DefaultTimeout
    )
    .AddHttpMessageHandler<ResilientHttpMessageHandler>();
builder.Services.AddApiServices();
builder.Services.AddSingleton<IValidator<CustomerIntakeForm>, CustomerIntakeFormValidator>();
builder.Services.AddSingleton<IValidationMessageLocalizer, ValidationMessageLocalizer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapApiEndpoints();
app.MapStaticAssets();
app.MapRazorComponents<FormValidationTest.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FormValidationTest.Client._Imports).Assembly);

app.Run();
