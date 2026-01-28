using System.Net;
using System.Net.Http.Json;
using App.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace App.Api.Tests;

public sealed class PrefillIntegrationDemoEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public PrefillIntegrationDemoEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Non_matching_name_returns_not_found_response()
    {
        var response = await _client.GetAsync(
            "/api/prefill-integration-demo?name=Jane",
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload =
            await response.Content.ReadFromJsonAsync<PrefillIntegrationDemoLookupResponse>(
                cancellationToken: CancellationToken
            );

        Assert.NotNull(payload);
        Assert.False(payload!.Found);
        Assert.Equal("Jane", payload.LookupName);
        Assert.Equal(PrefillIntegrationDemoDefaults.MatchingName, payload.MatchingName);
        Assert.Null(payload.Data);
    }

    [Fact]
    public async Task Matching_name_returns_prefill_data()
    {
        var response = await _client.GetAsync(
            $"/api/prefill-integration-demo?name={Uri.EscapeDataString(PrefillIntegrationDemoDefaults.MatchingName)}",
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload =
            await response.Content.ReadFromJsonAsync<PrefillIntegrationDemoLookupResponse>(
                cancellationToken: CancellationToken
            );

        Assert.NotNull(payload);
        Assert.True(payload!.Found);
        Assert.NotNull(payload.Data);
        Assert.Equal("123 Analytical Engine Way", payload.Data!.AddressLine1);
        Assert.Equal("Suite 42", payload.Data.AddressLine2);
        Assert.Equal("London", payload.Data.City);
        Assert.Equal("SW1A 1AA", payload.Data.PostalCode);
        Assert.Equal("+44 20 7946 0958", payload.Data.PhoneNumber);
        Assert.Equal("ada.lovelace@example.com", payload.Data.Email);
    }
}
