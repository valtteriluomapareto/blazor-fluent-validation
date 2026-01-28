# Implementing A New Form (Detailed Guide)
This is the end-to-end workflow for adding a brand-new form to the system, including shared contracts, validation, UI, and API.

## 1) Add shared contracts
Create a DTO in `src/App.Contracts/` for the new form model and, if needed, a response DTO.

Example: `src/App.Contracts/NewCustomerForm.cs`
```csharp
namespace App.Contracts;

public sealed class NewCustomerForm
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Seats { get; set; }
}
```

If the API returns a response:
```csharp
namespace App.Contracts;

public sealed record NewCustomerFormResponse(string Message);
```

## 2) Add validation rules (local + server)
Create a validator in `src/App.Validation/` and define explicit rule sets.

Example: `src/App.Validation/NewCustomerFormValidator.cs`
```csharp
using App.Contracts;
using FluentValidation;

namespace App.Validation;

public sealed class NewCustomerFormValidator : AbstractValidator<NewCustomerForm>
{
    public NewCustomerFormValidator()
    {
        RuleSet(
            "Local",
            () =>
            {
                RuleFor(x => x.Name)
                    .NotEmpty()
                    .WithErrorCode("new_customer.name.required")
                    .MaximumLength(120)
                    .WithErrorCode("new_customer.name.length");

                RuleFor(x => x.Email)
                    .NotEmpty()
                    .WithErrorCode("new_customer.email.required")
                    .EmailAddress()
                    .WithErrorCode("new_customer.email.invalid");

                RuleFor(x => x.Seats)
                    .InclusiveBetween(1, 5000)
                    .WithErrorCode("new_customer.seats.range");
            }
        );

        RuleSet(
            "Server",
            () =>
            {
                RuleFor(x => x.Email)
                    .Must(email => !email.EndsWith("@blocked.example", StringComparison.OrdinalIgnoreCase))
                    .WithErrorCode("new_customer.email.blocked")
                    .WithMessage("Email domain is blocked.");
            }
        );
    }
}
```

## 3) Add localized error messages
Map the error codes to Finnish messages in:
`src/App.Ui.Client/Services/Validation/ValidationMessageLocalizer.cs`

Example:
```csharp
["new_customer.name.required"] = "Nimi on pakollinen.",
["new_customer.name.length"] = "Nimi on liian pitkä.",
["new_customer.email.required"] = "Sähköposti on pakollinen.",
["new_customer.email.invalid"] = "Sähköposti on virheellinen.",
["new_customer.seats.range"] = "Paikkamäärä on sallittujen rajojen ulkopuolella.",
["new_customer.email.blocked"] = "Sähköpostin domain on estetty.",
```

## 4) Wire validator into the API
Register the validator and add a new endpoint in `src/App.Api/ApiModule.cs`.

Add in `AddApiServices`:
```csharp
services.AddSingleton<IValidator<NewCustomerForm>, NewCustomerFormValidator>();
```

Add a POST endpoint:
```csharp
app.MapPost(
        "/api/new-customer",
        (HttpContext httpContext, NewCustomerForm model) =>
        {
            // Optional endpoint-only checks
            return Results.Ok(new NewCustomerFormResponse("Form is valid."));
        }
    )
    .AddEndpointFilter(new ValidationFilter<NewCustomerForm>("Local", "Server"));
```

Notes:
- The `ValidationFilter<T>` runs FluentValidation and returns standardized errors.
- Use `"Local"` for client-safe rules and `"Server"` for server-only checks.

## 5) Create the UI page in the client
Add a page under `src/App.Ui.Client/Pages/`.

Example: `src/App.Ui.Client/Pages/NewCustomer.razor`
```razor
@page "/new-customer"
@rendermode InteractiveWebAssembly
@using App.Contracts
@implements IDisposable

<PageTitle>New Customer</PageTitle>

<FormCard>
    <EditForm EditContext="_editContext" OnSubmit="HandleSubmit" class="space-y-6">
        <LocalizedFluentValidator AsyncMode="true" RuleSets="@(new[] { "Local" })" />
        <ValidationSummary class="rounded-md border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700 empty:hidden" />

        <FormTextField Id="new-customer-name" Label="Name" UpdateOnInput="true" @bind-Value="_model.Name" />
        <FormTextField Id="new-customer-email" Label="Email" InputType="email" UpdateOnInput="true" @bind-Value="_model.Email" />
        <FormNumberField Id="new-customer-seats" Label="Seats" UpdateOnInput="true" @bind-Value="_model.Seats" />

        <button type="submit" class="inline-flex items-center rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white">
            Submit
        </button>

        @if (!string.IsNullOrWhiteSpace(_submitMessage))
        {
            <div class="rounded-md border border-sky-200 bg-sky-50 px-4 py-3 text-sm text-sky-800" role="status">
                @_submitMessage
            </div>
        }
    </EditForm>
</FormCard>

@code {
    private readonly NewCustomerForm _model = new();
    private EditContext _editContext = null!;
    private string? _submitMessage;
    private EventHandler<FieldChangedEventArgs>? _onFieldChanged;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(_model);
        _onFieldChanged = (_, _) => _submitMessage = null;
        _editContext.OnFieldChanged += _onFieldChanged;
    }

    private async Task HandleSubmit(EditContext editContext)
    {
        _submitMessage = null;
        var isValid = await editContext.ValidateAsync();
        if (!isValid)
        {
            _submitMessage = "Please fix the validation errors.";
            return;
        }

        _submitMessage = "Draft saved.";
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

## 6) Connect the UI to the API (optional)
If the form must submit to the API and surface server-side errors:
1. Inject `HttpClient` in the page.
2. POST to your endpoint.
3. Map `ValidationErrorResponse` to the `EditContext` errors and show localized messages.

Reference the existing page:
- `src/App.Ui.Client/Pages/SampleFormValidationWasm.razor`

## 7) Add navigation (if needed)
If the page needs to be discoverable in the UI, update the navigation shell in:
`src/App.Ui/Components/Layout/` or the relevant menu component.

## 8) Add tests
Recommended test coverage:
1. `tests/App.Validation.Tests` — validator rules, rule sets, and error codes.
2. `tests/App.Api.Tests` — endpoint returns and error response shape.
3. `tests/App.Ui.Client.Tests` — page render + invalid-then-fix flows.
4. `tests/App.E2E.Tests` — only for critical flows.

## 9) Run the full workflow
```bash
./scripts/test-fast.sh -c Release
./scripts/lint-fix.sh
```

