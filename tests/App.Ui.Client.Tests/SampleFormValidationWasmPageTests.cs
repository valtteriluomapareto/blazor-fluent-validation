using System.Net;
using System.Net.Http.Json;
using App.Abstractions;
using App.Contracts;
using App.Validation;
using FluentValidation;
using FormValidationTest.Client.Pages;
using FormValidationTest.Client.Services;
using FormValidationTest.Client.Services.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Ui.Client.Tests;

public sealed class SampleFormValidationWasmPageTests : IDisposable
{
    private readonly BunitContext context;

    public SampleFormValidationWasmPageTests()
    {
        context = new BunitContext();

        context.Services.AddSingleton<IUsedNameLookup, LocalUsedNameLookup>();
        context.Services.AddSingleton<IValidator<SampleForm>, SampleFormValidator>();
        context.Services.AddSingleton<IValidationMessageLocalizer, ValidationMessageLocalizer>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["Api:BaseUrl"] = "http://localhost/" }
            )
            .Build();
        context.Services.AddSingleton<IConfiguration>(configuration);

        var httpClient = new HttpClient(new ServerValidationHandler())
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        context.Services.AddSingleton(httpClient);
    }

    public void Dispose() => context.Dispose();

    [Fact]
    public void Server_error_codes_are_localized_in_ui()
    {
        var cut = context.Render<SampleFormValidationWasm>();

        cut.Find("input#name").Change("Jane");
        cut.Find("input#age").Change("30");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Nimi ei voi olla 'Server'.", cut.Markup);
        });
    }

    private sealed class ServerValidationHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            if (request.Method == HttpMethod.Post && request.RequestUri?.AbsolutePath == "/api/sample-form")
            {
                var errorResponse = new ValidationErrorResponse(
                    Title: "Validation failed.",
                    Status: 400,
                    Errors: new Dictionary<string, string[]>
                    {
                        ["Name"] = ["Name cannot be 'Server'."],
                    },
                    ErrorCodes: new Dictionary<string, string[]>
                    {
                        ["Name"] = ["name.server_reserved"],
                    },
                    Detail: "Server validation failed."
                );

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = JsonContent.Create(errorResponse),
                    }
                );
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
