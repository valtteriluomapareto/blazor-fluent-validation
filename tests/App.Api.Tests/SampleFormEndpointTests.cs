using System.Net;
using System.Net.Http.Json;
using App.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace App.Api.Tests;

public sealed class SampleFormEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public SampleFormEndpointTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Invalid_local_rules_return_validation_errors()
    {
        var model = new SampleForm { Name = "", Age = 10 };

        var response = await client.PostAsJsonAsync("/api/sample-form", model);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();

        Assert.NotNull(errors);
        Assert.Equal("Validation failed.", errors!.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, errors.Status);
        Assert.Contains("Name", errors.Errors.Keys);
        Assert.Contains("Age", errors.Errors.Keys);
    }

    [Fact]
    public async Task Server_rule_set_returns_error()
    {
        var model = new SampleForm { Name = "Server", Age = 30 };

        var response = await client.PostAsJsonAsync("/api/sample-form", model);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();

        Assert.NotNull(errors);
        Assert.Contains("Name", errors!.Errors.Keys);
        Assert.Contains("name.server_reserved", errors.ErrorCodes["Name"]);
    }

    [Fact]
    public async Task Used_name_returns_error()
    {
        var model = new SampleForm { Name = "Taken", Age = 30 };

        var response = await client.PostAsJsonAsync("/api/sample-form", model);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();

        Assert.NotNull(errors);
        Assert.Contains("Name", errors!.Errors.Keys);
        Assert.Contains("name.already_used", errors.ErrorCodes["Name"]);
    }

    [Fact]
    public async Task Endpoint_only_check_returns_error()
    {
        var model = new SampleForm { Name = "ApiOnly", Age = 30 };

        var response = await client.PostAsJsonAsync("/api/sample-form", model);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errors = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();

        Assert.NotNull(errors);
        Assert.Contains("Name", errors!.Errors.Keys);
        Assert.Contains("name.api_reserved", errors.ErrorCodes["Name"]);
    }

    [Fact]
    public async Task Valid_request_returns_success()
    {
        var model = new SampleForm { Name = "Jane", Age = 30 };

        var response = await client.PostAsJsonAsync("/api/sample-form", model);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<SampleFormResponse>();

        Assert.NotNull(payload);
        Assert.Equal("Form is valid.", payload!.Message);
    }
}
