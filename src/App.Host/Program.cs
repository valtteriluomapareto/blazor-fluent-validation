using App.Api;
using App.Contracts;
using App.Validation;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddHttpClient("Api");
builder.Services.AddApiServices();
builder.Services.AddSingleton<IValidator<CustomerIntakeForm>, CustomerIntakeFormValidator>();

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
