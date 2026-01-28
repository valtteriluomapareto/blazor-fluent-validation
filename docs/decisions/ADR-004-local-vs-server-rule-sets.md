# ADR-004: Local vs Server Validation Rule Sets

## Status

Accepted

## Date

2026-01

## Context

Some validation rules can run instantly (format checks, required fields, ranges), while others require external service calls (uniqueness checks, business rule lookups). Running async rules on every keystroke would be:
- Slow and disruptive to UX
- Expensive in terms of API calls
- Potentially inconsistent if external services are temporarily unavailable

We needed to split validation into:
- **Instant feedback**: Rules that run client-side on every change
- **Deferred validation**: Rules that run server-side on form submission

## Decision

Use **FluentValidation rule sets** to categorize rules:

1. **`"Local"` rule set**: Synchronous, deterministic rules that run in UI
2. **`"Server"` rule set**: Async rules requiring external lookups (API-only)
3. **Default (no rule set)**: Rules that run in both contexts

Configuration:
```csharp
// Validator definition
public class SampleFormValidator : AbstractValidator<SampleForm>
{
    public SampleFormValidator()
    {
        // Runs everywhere (Local + Server)
        RuleFor(x => x.Name).NotEmpty().WithErrorCode("NAME_REQUIRED");
        
        // Local only: instant UI feedback
        RuleSet("Local", () =>
        {
            RuleFor(x => x.Age).InclusiveBetween(18, 120).WithErrorCode("AGE_RANGE");
        });
        
        // Server only: requires external lookup
        RuleSet("Server", () =>
        {
            RuleFor(x => x.Name).MustAsync(BeUniqueName).WithErrorCode("NAME_TAKEN");
        });
    }
}

// UI configuration (Blazilla)
<LocalizedFluentValidator AsyncMode="true" RuleSets="@(new[] { "Local" })" />

// API runs all rule sets by default
```

## Consequences

### Positive

- **Fast UI feedback**: Local rules validate instantly without round-trips
- **Clean separation**: Async dependencies don't pollute client-side code
- **Flexible composition**: Can add/remove rule sets per context
- **Single validator class**: All rules in one place, organized by execution context

### Negative

- **Rule set discipline**: Developers must correctly tag rules
- **Potential gaps**: A rule in wrong set could cause UI/API mismatch
- **Testing complexity**: Need to test both rule set combinations

### Neutral

- Blazilla's `AsyncMode="true"` handles validation subscription lifecycle
- API validation filter runs all rules by default (no explicit rule set needed)

## Alternatives Considered

### Alternative 1: Separate Validator Classes

`SampleFormLocalValidator` and `SampleFormServerValidator`.

**Why rejected**: Duplication of common rules, harder to see complete validation picture, risk of drift.

### Alternative 2: Attribute-Based Markers

Custom attributes like `[LocalRule]`, `[ServerRule]`.

**Why rejected**: FluentValidation doesn't natively support this, would require custom infrastructure.

### Alternative 3: All Rules Run Everywhere

No distinction; all rules run on every validation.

**Why rejected**: Async rules would block UI, require network calls on every keystroke, poor UX.

## References

- [FluentValidation Rule Sets](https://docs.fluentvalidation.net/en/latest/rulesets.html)
- [docs/blazilla-usage.md](../blazilla-usage.md) — Blazilla integration guide
- [docs/new-form-guide.md](../new-form-guide.md) — Form implementation walkthrough
