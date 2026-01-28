# ADR-001: Validation-First Architecture

## Status

Accepted

## Date

2026-01

## Context

This application orchestrates external APIs without an internal database. The primary complexity lies in ensuring data integrity through comprehensive validation before making external API calls. We needed to decide how to structure validation logic across the stack.

Key requirements:
- Validation rules must be consistent between UI and API
- Rules should be testable in isolation
- Error messages must be stable and localizable
- Async validations (external lookups) should not block UI responsiveness

## Decision

Adopt a **validation-first architecture** where:

1. **Single source of truth**: All validation rules live in `App.Validation` using FluentValidation
2. **Shared validators**: Both UI and API reference the same validator classes
3. **Stable error codes**: Every rule emits a deterministic error code (not just messages)
4. **Property path alignment**: Error paths match DTO property names for automatic UI binding

The validation layer (`App.Validation`) depends only on `App.Contracts` (DTOs) and `App.Abstractions` (interfaces for async checks), ensuring rules remain pure and testable.

## Consequences

### Positive

- **No drift**: UI and API validation rules cannot diverge since they share the same code
- **High testability**: Validators are unit-tested in isolation with 95%+ coverage
- **Predictable errors**: Stable error codes enable localization and contract testing
- **Fast feedback**: Local rules run instantly in UI; async rules run on submit

### Negative

- **Learning curve**: Developers must understand FluentValidation's rule sets and async patterns
- **Abstraction overhead**: Async validation requires interfaces in `App.Abstractions`

### Neutral

- Validation is synchronous by default; async rules require explicit `RuleSet` tagging

## Alternatives Considered

### Alternative 1: Data Annotations

Using `[Required]`, `[StringLength]`, etc. on DTOs.

**Why rejected**: Limited expressiveness, no async support, error codes require custom adapters, conditional rules are awkward.

### Alternative 2: Separate UI and API Validators

Writing distinct validation logic for each layer.

**Why rejected**: High risk of drift, duplicate maintenance, inconsistent error messages.

### Alternative 3: API-Only Validation

No client-side validation; rely entirely on API responses.

**Why rejected**: Poor UX (round-trip for every validation), increased API load, no instant feedback.

## References

- [FluentValidation documentation](https://docs.fluentvalidation.net/)
- [specs/architecture-plan.md](../../specs/architecture-plan.md) — Validation Architecture section
- [docs/new-form-guide.md](../new-form-guide.md) — Step-by-step form implementation
