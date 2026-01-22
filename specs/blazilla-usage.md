# Blazilla Usage

## Install (UI project)
- Add the Blazilla package to the Blazor UI project.

## Register validators
- Register FluentValidation validators in DI as singletons.
- Example (Blazor Server):

```csharp
builder.Services.AddSingleton<IValidator<SampleForm>, SampleFormValidator>();
```

## Use in a Blazor component
- Place the Blazilla validator component inside the `EditForm`.

```razor
<EditForm Model="@model" OnValidSubmit="@HandleValidSubmit">
    <FluentValidator />
    <ValidationSummary />
    <!-- fields -->
</EditForm>
```

## Async validation rules
- When validators contain async rules, use `AsyncMode="true"` and handle submission with `OnSubmit` + `ValidateAsync()` on the `EditContext`.

```razor
<EditForm Model="@model" OnSubmit="@HandleSubmit">
    <FluentValidator AsyncMode="true" />
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
