using System.Text.Json;
using App.Abstractions;
using Microsoft.Extensions.Logging;

namespace App.Integrations;

public sealed class MockFormSubmissionIntegration : IFormSubmissionIntegration
{
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web
    );
    private readonly ILogger<MockFormSubmissionIntegration> _logger;

    public MockFormSubmissionIntegration(ILogger<MockFormSubmissionIntegration> logger)
    {
        _logger = logger;
    }

    public Task SubmitAsync<T>(
        string formName,
        T payload,
        CancellationToken cancellationToken = default
    )
    {
        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        _logger.LogInformation(
            "Mock integration received {FormName} submission: {Payload}",
            formName,
            json
        );
        return Task.CompletedTask;
    }
}
