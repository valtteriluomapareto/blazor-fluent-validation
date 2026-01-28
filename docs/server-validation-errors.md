# Server Validation Errors (End-to-End Guide)

This guide explains how server-side validation errors flow from the API to the UI and are displayed to users. The pattern ensures consistent error handling with localized messages.

## Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              DATA FLOW                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  UI (Blazor WASM)                    API (Minimal API)                      │
│  ────────────────                    ─────────────────                      │
│                                                                             │
│  1. User fills form                                                         │
│     ↓                                                                       │
│  2. Local validation (RuleSets="Local")                                     │
│     ↓ (passes)                                                              │
│  3. POST /api/form ──────────────────→ 4. ValidationFilter runs             │
│                                           - Local + Server rule sets        │
│                                           - Async rules (lookups, etc.)     │
│                                           ↓                                 │
│                                        5. Validation fails                  │
│                                           ↓                                 │
│  6. Receive 400 Bad Request ←──────── ValidationErrorResponse               │
│     ↓                                    {                                  │
│  7. Deserialize response                   "errors": { "Name": [...] },     │
│     ↓                                      "errorCodes": { "Name": [...] }  │
│  8. Localize error codes                 }                                  │
│     ↓                                                                       │
│  9. Add to ValidationMessageStore                                           │
│     ↓                                                                       │
│  10. UI re-renders with errors                                              │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

## The Error Contract

Server validation errors use a standardized response shape defined in `App.Contracts`:

```csharp
public sealed record ValidationErrorResponse(
    string Title,                           // "Validation failed."
    int Status,                             // 400
    Dictionary<string, string[]> Errors,    // Human-readable messages (fallback)
    Dictionary<string, string[]> ErrorCodes,// Stable codes for localization
    string? Type = null,                    // "https://httpstatuses.com/400"
    string? Detail = null,                  // Optional details
    string? Instance = null                 // Request path
);
```

Example API response:
```json
{
  "title": "Validation failed.",
  "status": 400,
  "errors": {
    "Name": ["Name cannot be 'Server'."]
  },
  "errorCodes": {
    "Name": ["name.server_reserved"]
  },
  "type": "https://httpstatuses.com/400",
  "instance": "/api/sample-form"
}
```

**Key design decision:** The API returns both `errors` (messages) and `errorCodes`. This allows:
- UI to localize using `errorCodes` (preferred)
- Fallback to `errors` when a code is unknown or missing

## Step-by-Step Implementation

### Step 1: Define validation rules with error codes

Every rule must emit a stable error code. Server-only rules go in the `"Server"` rule set:

```csharp
// src/App.Validation/SampleFormValidator.cs
public sealed class SampleFormValidator : AbstractValidator<SampleForm>
{
    public SampleFormValidator(IUsedNameLookup usedNameLookup)
    {
        // Local rules: run in UI and API
        RuleSet("Local", () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithErrorCode("name.required");

            RuleFor(x => x.Age)
                .InclusiveBetween(18, 120)
                .WithErrorCode("age.range");
        });

        // Server rules: run only in API (async lookups, reserved values, etc.)
        RuleSet("Server", () =>
        {
            RuleFor(x => x.Name)
                .NotEqual("Server")
                .WithErrorCode("name.server_reserved")
                .WithMessage("Name cannot be 'Server'.");

            RuleFor(x => x.Name)
                .MustAsync(async (name, ct) =>
                {
                    var usedNames = await usedNameLookup.GetUsedNamesAsync(ct);
                    return !usedNames.Contains(name, StringComparer.OrdinalIgnoreCase);
                })
                .WithErrorCode("name.already_used")
                .WithMessage("Name is already used.")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));
        });
    }
}
```

### Step 2: Wire the validation filter in the API

The `ValidationFilter<T>` runs FluentValidation and returns standardized errors:

```csharp
// src/App.Api/ApiModule.cs
app.MapPost("/api/sample-form", (HttpContext httpContext, SampleForm model) =>
    {
        // Business logic here (only reached if validation passes)
        return Results.Ok(new SampleFormResponse("Form is valid."));
    })
    .AddEndpointFilter(new ValidationFilter<SampleForm>("Local", "Server"));
```

The filter:
1. Resolves `IValidator<T>` from DI
2. Runs validation with specified rule sets
3. On failure, returns `ValidationErrorResponse` with both messages and codes

### Step 3: Add localized messages for error codes

Map error codes to localized strings in the UI client:

```csharp
// src/App.Ui.Client/Services/Validation/ValidationMessageLocalizer.cs
private static readonly IReadOnlyDictionary<string, string> FinnishMessages = new Dictionary<string, string>
{
    ["name.required"] = "Nimi on pakollinen.",
    ["name.server_reserved"] = "Nimi ei voi olla 'Server'.",
    ["name.already_used"] = "Nimi on jo käytössä.",
    ["age.range"] = "Iän tulee olla välillä 18–120.",
    // ... more mappings
};
```

### Step 4: Handle server errors in the UI page

The UI page needs to:
1. Submit to the API
2. Deserialize error responses
3. Map errors to the EditContext
4. Clear server errors when fields change

```razor
@* src/App.Ui.Client/Pages/SampleFormValidationWasm.razor *@
@page "/sample-form"
@rendermode InteractiveWebAssembly
@inject HttpClient Http
@inject IValidationMessageLocalizer ValidationMessageLocalizer
@implements IDisposable

<EditForm EditContext="_editContext" OnSubmit="HandleSubmit">
    <LocalizedFluentValidator AsyncMode="true" RuleSets="@(new[] { "Local" })" />
    <ValidationSummary />

    <FormTextField Id="name" Label="Name" @bind-Value="_model.Name" />
    <FormNumberField Id="age" Label="Age" @bind-Value="_model.Age" />

    <button type="submit">Submit</button>
</EditForm>

@code {
    private readonly SampleForm _model = new();
    private EditContext _editContext = null!;
    private ValidationMessageStore? _messageStore;
    private EventHandler<FieldChangedEventArgs>? _onFieldChanged;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(_model);
        _messageStore = new ValidationMessageStore(_editContext);

        // Clear server errors when user edits a field
        _onFieldChanged = (_, args) =>
        {
            _messageStore?.Clear(args.FieldIdentifier);
            _editContext.NotifyValidationStateChanged();
        };
        _editContext.OnFieldChanged += _onFieldChanged;
    }

    private async Task HandleSubmit(EditContext editContext)
    {
        // Clear previous server errors
        _messageStore?.Clear();
        editContext.NotifyValidationStateChanged();

        // Run local validation first
        var isValid = await editContext.ValidateAsync();
        if (!isValid)
        {
            return;
        }

        // Submit to API
        var response = await Http.PostAsJsonAsync("/api/sample-form", _model);

        if (response.IsSuccessStatusCode)
        {
            // Handle success
            return;
        }

        // Handle validation errors
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
            if (errorResponse is not null)
            {
                AddServerValidationErrors(errorResponse);
                _editContext.NotifyValidationStateChanged();
            }
        }
    }

    private void AddServerValidationErrors(ValidationErrorResponse errorResponse)
    {
        var handledFields = new HashSet<string>(StringComparer.Ordinal);

        // First, process error codes (preferred — enables localization)
        if (errorResponse.ErrorCodes is { Count: > 0 })
        {
            foreach (var (field, codes) in errorResponse.ErrorCodes)
            {
                // Get fallback messages for this field
                errorResponse.Errors.TryGetValue(field, out var fallbackMessages);

                // Localize the error codes
                var localizedMessages = ValidationMessageLocalizer.LocalizeMany(codes, fallbackMessages);
                if (localizedMessages.Count == 0) continue;

                // Add to the validation message store
                _messageStore?.Add(new FieldIdentifier(_model, field), localizedMessages);
                handledFields.Add(field);
            }
        }

        // Then, fall back to raw messages for fields without error codes
        if (errorResponse.Errors is { Count: > 0 })
        {
            foreach (var (field, messages) in errorResponse.Errors)
            {
                if (handledFields.Contains(field)) continue;

                _messageStore?.Add(new FieldIdentifier(_model, field), messages);
            }
        }
    }

    public void Dispose()
    {
        if (_onFieldChanged is not null)
        {
            _editContext.OnFieldChanged -= _onFieldChanged;
        }
    }
}
```

## Key Patterns Explained

### ValidationMessageStore

The `ValidationMessageStore` holds validation messages associated with an `EditContext`. Unlike the built-in validation from `<FluentValidator>`, server errors must be manually added:

```csharp
// Create the store (once, in OnInitialized)
_messageStore = new ValidationMessageStore(_editContext);

// Add errors for a specific field
_messageStore.Add(new FieldIdentifier(_model, "Name"), ["Error message"]);

// Clear errors for a specific field
_messageStore.Clear(new FieldIdentifier(_model, "Name"));

// Clear all errors
_messageStore.Clear();

// Notify UI to re-render
_editContext.NotifyValidationStateChanged();
```

### Clearing server errors on field change

Server errors should disappear when the user edits the field. Subscribe to `OnFieldChanged`:

```csharp
_editContext.OnFieldChanged += (_, args) =>
{
    _messageStore?.Clear(args.FieldIdentifier);  // Clear only this field
    _editContext.NotifyValidationStateChanged();
};
```

### Error code localization with fallback

The localizer tries error codes first, falls back to server messages:

```csharp
// If "name.server_reserved" exists in Finnish mappings, use it
// Otherwise, use the server-provided message "Name cannot be 'Server'."
var localizedMessages = ValidationMessageLocalizer.LocalizeMany(codes, fallbackMessages);
```

## Testing Server Validation Errors

### Unit test (bUnit)

Mock the HTTP client to return validation errors:

```csharp
// tests/App.Ui.Client.Tests/SampleFormValidationWasmPageTests.cs
[Fact]
public void Server_error_codes_are_localized_in_ui()
{
    var cut = context.Render<SampleFormValidationWasm>();

    // Fill form with valid local data
    cut.Find("input#name").Change("Jane");
    cut.Find("input#age").Change("30");

    // Submit (mock handler returns server validation error)
    cut.Find("form").Submit();

    // Verify localized message appears
    cut.WaitForAssertion(() =>
    {
        Assert.Contains("Nimi ei voi olla 'Server'.", cut.Markup);
    });
}

private sealed class ServerValidationHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var errorResponse = new ValidationErrorResponse(
            Title: "Validation failed.",
            Status: 400,
            Errors: new Dictionary<string, string[]>
            {
                ["Name"] = ["Name cannot be 'Server'."],
            },
            ErrorCodes: new Dictionary<string, string[]>
            {
                ["Name"] = ["name.server_reserved"],
            },
            Detail: "Server validation failed."
        );

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(errorResponse),
        });
    }
}
```

### API test

Verify the API returns the correct error shape:

```csharp
// tests/App.Api.Tests/SampleFormEndpointTests.cs
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
```

### E2E test (Playwright)

Test the full flow in a real browser:

```csharp
// tests/App.E2E.Tests/SampleFormE2ETests.cs
[Fact]
public async Task Server_validation_error_displays_in_ui()
{
    await Page.GotoAsync("/sample-form");

    // Fill with data that triggers server-only validation
    await Page.GetByLabel("Name").FillAsync("Server");
    await Page.GetByLabel("Age").FillAsync("30");

    // Submit
    await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

    // Verify localized error appears
    await Expect(Page.GetByText("Nimi ei voi olla 'Server'.")).ToBeVisibleAsync();
}
```

## Common Failure Scenarios

| Scenario | Trigger | Expected Behavior |
|----------|---------|-------------------|
| Server-reserved name | Name = "Server" | Error: "Nimi ei voi olla 'Server'." |
| Name already used | Name = "Taken" | Error: "Nimi on jo käytössä." |
| Endpoint-only check | Name = "ApiOnly" | Error: "Nimi ei voi olla 'ApiOnly'." |
| Unknown error code | Code not in localizer | Falls back to server message |
| Network failure | API unreachable | "Could not reach the API at {url}." |

## Reference Implementation

- **UI page:** `src/App.Ui.Client/Pages/SampleFormValidationWasm.razor`
- **Validator:** `src/App.Validation/SampleFormValidator.cs`
- **API endpoint:** `src/App.Api/ApiModule.cs`
- **Localizer:** `src/App.Ui.Client/Services/Validation/ValidationMessageLocalizer.cs`
- **Error contract:** `src/App.Contracts/ValidationErrorResponse.cs`
- **Validation filter:** `src/App.Api/Validation/ValidationFilter.cs`
- **bUnit tests:** `tests/App.Ui.Client.Tests/SampleFormValidationWasmPageTests.cs`
- **API tests:** `tests/App.Api.Tests/SampleFormEndpointTests.cs`
