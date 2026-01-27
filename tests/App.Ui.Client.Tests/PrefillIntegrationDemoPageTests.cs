using System.Net;
using System.Net.Http.Json;
using App.Contracts;
using App.Validation;
using FluentValidation;
using FormValidationTest.Client.Pages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Ui.Client.Tests;

public sealed class PrefillIntegrationDemoPageTests : IDisposable
{
    private readonly BunitContext context = new();

    public PrefillIntegrationDemoPageTests()
    {
        context.Services.AddSingleton<
            IValidator<PrefillIntegrationDemoForm>,
            PrefillIntegrationDemoFormValidator
        >();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["Api:BaseUrl"] = "http://localhost/" }
            )
            .Build();
        context.Services.AddSingleton<IConfiguration>(configuration);

        var httpClient = new HttpClient(new StubPrefillHandler())
        {
            BaseAddress = new Uri("http://localhost/"),
        };
        context.Services.AddSingleton(httpClient);
    }

    public void Dispose() => context.Dispose();

    [Fact]
    public void PrefillIntegrationDemo_renders_expected_fields()
    {
        var cut = context.Render<PrefillIntegrationDemo>();

        Assert.NotNull(cut.Find("input#prefill-name"));
        Assert.NotNull(cut.Find("input#prefill-address-line1"));
        Assert.NotNull(cut.Find("input#prefill-address-line2"));
        Assert.NotNull(cut.Find("input#prefill-city"));
        Assert.NotNull(cut.Find("input#prefill-postal-code"));
        Assert.NotNull(cut.Find("input#prefill-phone"));
        Assert.NotNull(cut.Find("input#prefill-email"));
    }

    [Fact]
    public void Matching_name_prefills_contact_fields()
    {
        var cut = context.Render<PrefillIntegrationDemo>();

        cut.Find("input#prefill-name").Change(PrefillIntegrationDemoDefaults.MatchingName);

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                "123 Analytical Engine Way",
                cut.Find("input#prefill-address-line1").GetAttribute("value")
            );
            Assert.Equal("Suite 42", cut.Find("input#prefill-address-line2").GetAttribute("value"));
            Assert.Equal("London", cut.Find("input#prefill-city").GetAttribute("value"));
            Assert.Equal("SW1A 1AA", cut.Find("input#prefill-postal-code").GetAttribute("value"));
            Assert.Equal("+44 20 7946 0958", cut.Find("input#prefill-phone").GetAttribute("value"));
            Assert.Equal(
                "ada.lovelace@example.com",
                cut.Find("input#prefill-email").GetAttribute("value")
            );
        });
    }

    private sealed class StubPrefillHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            if (request.RequestUri is null)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
            }

            var lookupName = GetLookupName(request.RequestUri);
            var isMatch = string.Equals(
                lookupName,
                PrefillIntegrationDemoDefaults.MatchingName,
                StringComparison.OrdinalIgnoreCase
            );

            var payload = isMatch
                ? new PrefillIntegrationDemoLookupResponse(
                    Found: true,
                    LookupName: lookupName,
                    MatchingName: PrefillIntegrationDemoDefaults.MatchingName,
                    Data: new PrefillIntegrationDemoPrefillData
                    {
                        AddressLine1 = "123 Analytical Engine Way",
                        AddressLine2 = "Suite 42",
                        City = "London",
                        PostalCode = "SW1A 1AA",
                        PhoneNumber = "+44 20 7946 0958",
                        Email = "ada.lovelace@example.com",
                    },
                    Message: "Integration returned existing data."
                )
                : new PrefillIntegrationDemoLookupResponse(
                    Found: false,
                    LookupName: lookupName,
                    MatchingName: PrefillIntegrationDemoDefaults.MatchingName,
                    Data: null,
                    Message: "No integration data found for that name."
                );

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(payload),
            };

            return Task.FromResult(response);
        }

        private static string GetLookupName(Uri uri)
        {
            var query = uri.Query;
            const string prefix = "?name=";
            if (!query.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            var encoded = query[prefix.Length..];
            return Uri.UnescapeDataString(encoded).Trim();
        }
    }
}
