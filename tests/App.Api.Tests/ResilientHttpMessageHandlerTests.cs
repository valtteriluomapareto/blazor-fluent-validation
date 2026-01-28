using System.Net;
using System.Net.Http.Headers;
using App.Api.Http;
using Xunit;

namespace App.Api.Tests;

public sealed class ResilientHttpMessageHandlerTests
{
    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Retries_on_server_error_and_returns_success()
    {
        var handler = new ResilientHttpMessageHandler
        {
            InnerHandler = new SequenceHandler(
                CreateResponse(HttpStatusCode.InternalServerError, retryAfterZero: true),
                CreateResponse(HttpStatusCode.InternalServerError, retryAfterZero: true),
                CreateResponse(HttpStatusCode.OK)
            ),
        };

        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");

        using var response = await client.SendAsync(request, CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, handler.InnerHandler.As<SequenceHandler>().CallCount);
    }

    [Fact]
    public async Task Stops_after_max_retries_and_returns_last_response()
    {
        var handler = new ResilientHttpMessageHandler
        {
            InnerHandler = new SequenceHandler(
                CreateResponse(HttpStatusCode.BadGateway, retryAfterZero: true),
                CreateResponse(HttpStatusCode.BadGateway, retryAfterZero: true),
                CreateResponse(HttpStatusCode.BadGateway, retryAfterZero: true)
            ),
        };

        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");

        using var response = await client.SendAsync(request, CancellationToken);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal(3, handler.InnerHandler.As<SequenceHandler>().CallCount);
    }

    [Fact]
    public async Task Does_not_retry_non_retryable_status()
    {
        var handler = new ResilientHttpMessageHandler
        {
            InnerHandler = new SequenceHandler(CreateResponse(HttpStatusCode.BadRequest)),
        };

        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");

        using var response = await client.SendAsync(request, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(1, handler.InnerHandler.As<SequenceHandler>().CallCount);
    }

    [Fact]
    public async Task Does_not_retry_requests_with_content()
    {
        var handler = new ResilientHttpMessageHandler
        {
            InnerHandler = new SequenceHandler(
                CreateResponse(HttpStatusCode.InternalServerError, retryAfterZero: true),
                CreateResponse(HttpStatusCode.OK)
            ),
        };

        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://example.test/")
        {
            Content = new StringContent("payload"),
        };

        using var response = await client.SendAsync(request, CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(1, handler.InnerHandler.As<SequenceHandler>().CallCount);
    }

    [Fact]
    public async Task Retries_on_too_many_requests()
    {
        var handler = new ResilientHttpMessageHandler
        {
            InnerHandler = new SequenceHandler(
                CreateResponse(HttpStatusCode.TooManyRequests, retryAfterZero: true),
                CreateResponse(HttpStatusCode.OK)
            ),
        };

        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");

        using var response = await client.SendAsync(request, CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, handler.InnerHandler.As<SequenceHandler>().CallCount);
    }

    private static HttpResponseMessage CreateResponse(
        HttpStatusCode statusCode,
        bool retryAfterZero = false
    )
    {
        var response = new HttpResponseMessage(statusCode);
        if (retryAfterZero)
        {
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);
        }

        return response;
    }

    private sealed class SequenceHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> responses;
        private int callCount;

        public SequenceHandler(params HttpResponseMessage[] responses)
        {
            this.responses = new Queue<HttpResponseMessage>(responses);
        }

        public int CallCount => callCount;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            callCount++;

            if (!responses.TryDequeue(out var response))
            {
                throw new InvalidOperationException("No more responses configured.");
            }

            return Task.FromResult(response);
        }
    }
}

internal static class HandlerExtensions
{
    public static T As<T>(this HttpMessageHandler handler)
        where T : HttpMessageHandler
    {
        return handler as T
            ?? throw new InvalidOperationException(
                $"Expected handler to be {typeof(T).Name} but was {handler.GetType().Name}."
            );
    }
}
