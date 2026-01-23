# Blazilla Usage

## Install (UI project)
- Add the Blazilla package to the Blazor UI project.

## Register validators
- Register FluentValidation validators in DI as singletons.
- Example (Blazor Server):

```csharp
builder.Services.AddSingleton<IValidator<SampleForm>, SampleFormValidator>();
```

## Use in a Blazor component (sync-only validators)
- If the validator has no async rules, you can use the standard synchronous flow.

```razor
<EditForm Model="@model" OnValidSubmit="@HandleValidSubmit">
    <FluentValidator />
    <ValidationSummary />
    <!-- fields -->
</EditForm>
```

## Async validation rules
- When validators contain async rules, use `AsyncMode="true"` and handle submission with `OnSubmit` + `ValidateAsync()` on the `EditContext`.
- This is the approach used in the sample form so we can keep a single validator while still supporting async rules.
- See `src/App.Ui/Components/Pages/SampleFormValidation.razor` for the live example in this repo.

```razor
<EditForm EditContext="@editContext" OnSubmit="@HandleSubmit">
    <FluentValidator AsyncMode="true" RuleSets="@(new[] { "Local" })" />
    <ValidationSummary />
    <!-- fields -->
</EditForm>
```

```csharp
private async Task HandleSubmit(EditContext editContext)
{
    var isValid = await editContext.ValidateAsync();
    if (isValid)
    {
        // submit
    }
}
```

## Rule sets (optional)
- Use `RuleSets` to execute specific validation groups or `AllRules="true"` to run everything.

```razor
<FluentValidator RuleSets="@(new[] { "Create" })" />
```
