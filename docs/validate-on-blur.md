# Validate On Blur

This guide explains the optional "validate on blur" feature for form fields, which shows validation errors when a user leaves a field without making changes.

## Background

By default, Blazor form validation triggers when:
1. **Field value changes** — validation runs for that field
2. **Form submits** — full validation runs

This means if a user:
1. Clicks into an empty required field
2. Doesn't type anything
3. Tabs to the next field

No error appears because the value didn't change — it was empty before and still empty.

## The ValidateOnBlur Option

The `ValidateOnBlur` parameter enables "touched" validation: when a user focuses a field and then leaves it (blur), validation runs even if the value didn't change.

### Supported Components

| Component | ValidateOnBlur Support |
|-----------|----------------------|
| `FormTextField` | Yes |
| `FormTextAreaField` | Yes |
| `FormNumberField` | Not yet (see "Adding to Other Components") |
| `FormDecimalField` | Not yet |
| `FormDateField` | Not yet |
| `FormSelectEnumField` | Not yet |

### Usage

```razor
<FormTextField 
    Id="name" 
    Label="Name" 
    ValidateOnBlur="true"
    @bind-Value="_model.Name" />

<FormTextAreaField 
    Id="notes" 
    Label="Notes" 
    ValidateOnBlur="true"
    @bind-Value="_model.Notes" />
```

### Behavior

| Scenario | Without ValidateOnBlur | With ValidateOnBlur |
|----------|----------------------|-------------------|
| Focus empty required field, blur without typing | No error | Error appears |
| Focus field, type invalid value, blur | Error appears | Error appears |
| Focus field, type valid value, blur | No error | No error |

### How It Works

1. **On focus**: The component records the current value and marks itself as "touched"
2. **On blur**: If `ValidateOnBlur` is enabled and the value hasn't changed since focus, the component calls `EditContext.NotifyFieldChanged()` to trigger validation
3. **Avoids double validation**: If the user typed something (value changed), validation already ran via the normal `OnFieldChanged` flow, so the blur handler skips triggering it again

## Adding ValidateOnBlur to Other Components

To add this feature to other form field components, follow this pattern:

### Step 1: Add the parameter and state

```csharp
@code {
    [CascadingParameter] private EditContext? EditContext { get; set; }
    
    [Parameter] public bool ValidateOnBlur { get; set; }
    
    private bool _touched;
    private TValue? _valueOnFocus;  // Use the appropriate type
    
    // ... existing code
}
```

### Step 2: Add event handlers to the input element

```razor
<InputText
    ...
    @onfocus="HandleFocus"
    @onfocusout="HandleBlur"
    ... />
```

### Step 3: Implement the handlers

```csharp
private void HandleFocus()
{
    _touched = true;
    _valueOnFocus = Value;
}

private void HandleBlur()
{
    if (!ValidateOnBlur || !_touched || EditContext is null || ValueExpression is null)
    {
        return;
    }

    // Only trigger validation on blur if value didn't change since focus.
    // If value changed, validation already ran via EditContext.OnFieldChanged.
    if (EqualityComparer<TValue>.Default.Equals(Value, _valueOnFocus))
    {
        var fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        EditContext.NotifyFieldChanged(fieldIdentifier);
    }
}
```

For string fields, use `string.Equals(Value, _valueOnFocus, StringComparison.Ordinal)` instead of `EqualityComparer`.

### Step 4: Add tests

Add tests to verify:
1. Blur triggers validation for untouched empty fields
2. Blur doesn't double-validate when value changed
3. Feature is disabled by default

See `tests/App.Ui.Client.Tests/FormComponentsTests.cs` for examples:
- `FormTextField_validate_on_blur_triggers_validation_for_untouched_empty_field`
- `FormTextField_validate_on_blur_does_not_double_validate_when_value_changed`
- `FormTextField_validate_on_blur_disabled_by_default`

## Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Default value | `false` | Backward compatible; opt-in for new behavior |
| Skip if value changed | Yes | Avoid double validation when user types then blurs |
| Track touched state | Yes | Only validate fields user has actually interacted with |
| Requires ValueExpression | Yes | Needed to create the correct `FieldIdentifier` |

## Example: Full Form with ValidateOnBlur

```razor
@page "/customer-form"
@rendermode InteractiveWebAssembly
@implements IDisposable

<EditForm EditContext="_editContext" OnSubmit="HandleSubmit">
    <LocalizedFluentValidator AsyncMode="true" RuleSets="@(new[] { "Local" })" />
    <ValidationSummary />

    <FormTextField 
        Id="name" 
        Label="Customer name" 
        ValidateOnBlur="true"
        @bind-Value="_model.CustomerName" />

    <FormTextField 
        Id="email" 
        Label="Contact email" 
        InputType="email"
        ValidateOnBlur="true"
        @bind-Value="_model.ContactEmail" />

    <FormTextAreaField 
        Id="notes" 
        Label="Notes" 
        ValidateOnBlur="true"
        @bind-Value="_model.Notes" />

    <button type="submit">Submit</button>
</EditForm>

@code {
    private readonly CustomerIntakeForm _model = new();
    private EditContext _editContext = null!;

    protected override void OnInitialized()
    {
        _editContext = new EditContext(_model);
    }

    private async Task HandleSubmit(EditContext editContext)
    {
        var isValid = await editContext.ValidateAsync();
        if (!isValid) return;
        
        // Submit logic
    }

    public void Dispose() { }
}
```

## Reference

- **FormTextField implementation**: `src/App.Ui.Client/Components/Forms/FormTextField.razor`
- **FormTextAreaField implementation**: `src/App.Ui.Client/Components/Forms/FormTextAreaField.razor`
- **Tests**: `tests/App.Ui.Client.Tests/FormComponentsTests.cs`
