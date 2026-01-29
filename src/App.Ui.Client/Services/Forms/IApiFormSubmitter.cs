using Microsoft.AspNetCore.Components.Forms;

namespace FormValidationTest.Client.Services.Forms;

public interface IApiFormSubmitter
{
    Uri ApiBaseUri { get; }
    string ApiBaseUrl { get; }

    Task<FormSubmitResult> SubmitAsync<TModel>(
        string endpoint,
        TModel model,
        EditContext editContext,
        ValidationMessageStore messageStore,
        string? successFallbackMessage = null,
        CancellationToken cancellationToken = default
    )
        where TModel : class;
}
