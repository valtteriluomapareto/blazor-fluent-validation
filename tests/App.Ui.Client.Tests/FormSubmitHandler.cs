using System.Net;
using System.Net.Http.Json;
using App.Contracts;

namespace App.Ui.Client.Tests;

public sealed class FormSubmitHandler : HttpMessageHandler
{
    private readonly IReadOnlyDictionary<string, string> _responses;

    public FormSubmitHandler(IReadOnlyDictionary<string, string> responses)
    {
        _responses = responses;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (
            request.Method == HttpMethod.Post
            && request.RequestUri is not null
            && _responses.TryGetValue(request.RequestUri.AbsolutePath, out var message)
        )
        {
            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new FormSubmissionResponse(message)),
                }
            );
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
