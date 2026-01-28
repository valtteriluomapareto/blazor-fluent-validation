using System.Net;

namespace FormValidationTest.Client.Services.Http;

public sealed class ResilientHttpMessageHandler : DelegatingHandler
{
    public const string ApiClientName = "Api";
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    private const int MaxRetries = 2;
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMilliseconds(200),
        TimeSpan.FromMilliseconds(500),
    ];

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (!IsRetryableRequest(request))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        for (var attempt = 0; attempt <= MaxRetries; attempt++)
        {
            using var requestClone = CloneRequest(request);
            HttpResponseMessage? response = null;

            try
            {
                response = await base.SendAsync(requestClone, cancellationToken);

                if (!ShouldRetry(response) || attempt == MaxRetries)
                {
                    return response;
                }
            }
            catch (HttpRequestException) when (attempt < MaxRetries)
            {
                // Swallow and retry.
            }

            response?.Dispose();

            var delay = GetDelay(attempt, response);
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private static bool IsRetryableRequest(HttpRequestMessage request)
    {
        if (request.Content is not null)
        {
            return false;
        }

        return request.Method == HttpMethod.Get
            || request.Method == HttpMethod.Head
            || request.Method == HttpMethod.Options;
    }

    private static bool ShouldRetry(HttpResponseMessage response)
    {
        if ((int)response.StatusCode >= 500)
        {
            return true;
        }

        return response.StatusCode
            is HttpStatusCode.RequestTimeout
                or HttpStatusCode.TooManyRequests;
    }

    private static TimeSpan GetDelay(int attempt, HttpResponseMessage? response)
    {
        var retryAfter = response?.Headers?.RetryAfter;
        if (retryAfter is not null)
        {
            if (retryAfter.Delta is { } delta)
            {
                return delta;
            }

            if (retryAfter.Date is { } date)
            {
                var delay = date - DateTimeOffset.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    return delay;
                }
            }
        }

        if (attempt < RetryDelays.Length)
        {
            return RetryDelays[attempt];
        }

        return RetryDelays[^1];
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy,
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
