# ADR-003: Error Codes for Localization

## Status

Accepted

## Date

2026-01

## Context

Validation error messages need to be displayed to users in their preferred language. The traditional approach of embedding messages directly in validators creates localization challenges:
- Messages are scattered across validator classes
- Changing a message requires code changes and redeployment
- Different languages require different validators or complex message providers

We needed a strategy that:
- Keeps validation rules stable and testable
- Enables UI-side localization without validator changes
- Supports fallback behavior when translations are missing
- Works consistently for both client-side and server-side validation errors

## Decision

Adopt **error codes as the stable contract**:

1. **Every validation rule emits an error code** via `.WithErrorCode("RULE_NAME")`
2. **API returns both `errors` and `errorCodes`** in `ValidationErrorResponse`
3. **UI maps error codes to localized strings** via `IValidationMessageLocalizer`
4. **Fallback behavior**: If a code has no translation, display the original message

Error code conventions:
- Format: `PROPERTY_RULE` (e.g., `NAME_REQUIRED`, `AGE_RANGE`)
- Codes are stable identifiers; messages are presentation concerns
- Codes are tested in `App.Validation.Tests` to prevent accidental changes

```csharp
// Validator
RuleFor(x => x.Name)
    .NotEmpty()
    .WithErrorCode("NAME_REQUIRED")
    .WithMessage("Name is required");

// API response
{
  "errors": { "Name": ["Name is required"] },
  "errorCodes": { "Name": ["NAME_REQUIRED"] }
}

// UI localization
var message = localizer.GetMessage("NAME_REQUIRED", fallback: "Name is required");
```

## Consequences

### Positive

- **Stable contracts**: Error codes don't change when messages are reworded
- **Testable**: API tests verify error codes, not fragile message text
- **Decoupled localization**: Translations managed in UI resources, not validators
- **Consistent UX**: Same code produces same localized message everywhere

### Negative

- **Dual output**: API returns both codes and messages (slightly larger payloads)
- **Mapping overhead**: UI must maintain code-to-message mappings
- **Discipline required**: Developers must remember to add `.WithErrorCode()`

### Neutral

- Existing messages serve as English fallbacks, easing migration

## Alternatives Considered

### Alternative 1: Localized Messages in Validators

Use `IStringLocalizer` directly in validators.

**Why rejected**: Validators become environment-dependent, harder to unit test, localization concerns leak into domain logic.

### Alternative 2: Message-Only Response

Return only `errors` dictionary with localized messages from API.

**Why rejected**: API would need to know user's locale, breaks stateless API design, testing becomes locale-dependent.

### Alternative 3: Numeric Error Codes

Use integers (e.g., `1001`, `1002`) instead of strings.

**Why rejected**: Less readable, requires lookup table, string codes are self-documenting.

## References

- [docs/server-validation-errors.md](../server-validation-errors.md) — End-to-end error handling guide
- [src/App.Contracts/ValidationErrorResponse.cs](../../src/App.Contracts/ValidationErrorResponse.cs) — Error contract DTO
- [specs/architecture-plan.md](../../specs/architecture-plan.md) — Localization Strategy section
