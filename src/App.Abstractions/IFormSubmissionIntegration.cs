namespace App.Abstractions;

public interface IFormSubmissionIntegration
{
    Task SubmitAsync<T>(string formName, T payload, CancellationToken cancellationToken = default);
}
