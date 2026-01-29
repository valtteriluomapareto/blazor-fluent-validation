using System.Net;
using System.Net.Http.Json;
using App.Contracts;
using Blazilla;
using FormValidationTest.Client.Services.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FormValidationTest.Client.Services.Forms;

public sealed class ApiFormSubmitter : IApiFormSubmitter
{
    private readonly IConfiguration _configuration;
    private readonly NavigationManager _navigationManager;
    private readonly HttpClient _http;
    private readonly IValidationMessageLocalizer _validationMessageLocalizer;

    public ApiFormSubmitter(
        IConfiguration configuration,
        NavigationManager navigationManager,
        HttpClient http,
        IValidationMessageLocalizer validationMessageLocalizer
    )
    {
        _configuration = configuration;
        _navigationManager = navigationManager;
        _http = http;
        _validationMessageLocalizer = validationMessageLocalizer;
    }

    public Uri ApiBaseUri => GetApiBaseUri();

    public string ApiBaseUrl => ApiBaseUri.GetLeftPart(UriPartial.Authority);

    public async Task<FormSubmitResult> SubmitAsync<TModel>(
        string endpoint,
        TModel model,
        EditContext editContext,
        ValidationMessageStore messageStore,
        string? successFallbackMessage = null,
        CancellationToken cancellationToken = default
    )
        where TModel : class
    {
        messageStore.Clear();
        editContext.NotifyValidationStateChanged();

        var isValid = await editContext.ValidateAsync();
        if (!isValid)
        {
            return new FormSubmitResult(true, "Please fix the validation errors.");
        }

        var apiBaseUri = GetApiBaseUri();

        try
        {
            var response = await _http.PostAsJsonAsync(
                new Uri(apiBaseUri, endpoint),
                model,
                cancellationToken
            );

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<FormSubmissionResponse>(
                    cancellationToken: cancellationToken
                );
                return new FormSubmitResult(
                    false,
                    apiResponse?.Message ?? successFallbackMessage ?? "Form submitted."
                );
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorResponse =
                    await response.Content.ReadFromJsonAsync<ValidationErrorResponse>(
                        cancellationToken: cancellationToken
                    );
                if (
                    errorResponse is not null
                    && AddServerValidationErrors(model, messageStore, errorResponse)
                )
                {
                    editContext.NotifyValidationStateChanged();
                    return new FormSubmitResult(true, "Please fix the validation errors.");
                }
            }

            return new FormSubmitResult(false, "Server error. Please try again.");
        }
        catch (HttpRequestException)
        {
            return new FormSubmitResult(false, $"Could not reach the API at {apiBaseUri}.");
        }
    }

    private Uri GetApiBaseUri()
    {
        var configuredBaseUrl = _configuration["Api:BaseUrl"];
        var baseUrl = string.IsNullOrWhiteSpace(configuredBaseUrl)
            ? _navigationManager.BaseUri
            : configuredBaseUrl;

        return new Uri(baseUrl, UriKind.Absolute);
    }

    private bool AddServerValidationErrors<TModel>(
        TModel model,
        ValidationMessageStore messageStore,
        ValidationErrorResponse errorResponse
    )
        where TModel : class
    {
        var addedAny = false;
        var handledFields = new HashSet<string>(StringComparer.Ordinal);

        if (errorResponse.ErrorCodes is { Count: > 0 })
        {
            foreach (var (field, codes) in errorResponse.ErrorCodes)
            {
                errorResponse.Errors.TryGetValue(field, out var fallbackMessages);
                var localizedMessages = _validationMessageLocalizer.LocalizeMany(
                    codes,
                    fallbackMessages
                );
                if (localizedMessages.Count == 0)
                {
                    continue;
                }

                messageStore.Add(new FieldIdentifier(model, field), localizedMessages);
                handledFields.Add(field);
                addedAny = true;
            }
        }

        if (errorResponse.Errors is { Count: > 0 })
        {
            foreach (var (field, messages) in errorResponse.Errors)
            {
                if (handledFields.Contains(field))
                {
                    continue;
                }

                messageStore.Add(new FieldIdentifier(model, field), messages);
                addedAny = true;
            }
        }

        return addedAny;
    }
}
