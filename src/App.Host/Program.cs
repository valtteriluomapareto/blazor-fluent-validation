using App.Api;
using FormValidationTest.Components;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddHttpClient("Api");
builder.Services.AddApiServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapApiEndpoints();
app.MapStaticAssets();
app.MapRazorComponents<FormValidationTest.Components.App>().AddInteractiveServerRenderMode();

app.Run();
