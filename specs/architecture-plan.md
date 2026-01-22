# Architecture Plan (Validation-First, C#-Only Stack)

## Goals
- C#-only developer experience using Blazor (no SPA frameworks).
- Single source of truth for validation rules shared by UI and API.
- High confidence in changes through layered testing (validation unit tests, API integration tests, minimal E2E).
- No internal database; orchestration of external APIs with robust validation and error handling.

## Key Principles
- Separation by responsibility: contracts, validation, integrations, API, UI.
- Explicit dependency direction; no cycles.
- Validation rules are stable, testable, and deterministic.
- External integrations are isolated behind adapters and normalized error contracts.

## Solution Structure (Projects)

### `App.Contracts`
**Purpose**
- Shared request/response DTOs.
- Shared enums/value types.
- Shared error contract types.

**Rules**
- No dependency on UI/API/Integration projects.
- No vendor DTOs.

### `App.Abstractions` (recommended)
**Purpose**
- Small interfaces required by validation or API orchestration (e.g., lookup interfaces).
- Shared by `App.Validation` and `App.Api` without dragging UI concerns into integrations.

**Rules**
- No references to integrations or infrastructure.

### `App.Validation`
**Purpose**
- FluentValidation validators for `App.Contracts` DTOs.
- Reusable rule helpers and error code conventions.

**Async Validation**
- Validators may depend on narrow interfaces from `App.Abstractions` for async checks.

**Rules**
- Depends on `App.Contracts` and `App.Abstractions`.
- No references to `App.Api` or `App.Ui`.

### `App.Integrations` (or `App.Infrastructure`)
**Purpose**
- HTTP clients and adapters for external APIs.
- Vendor DTOs and mapping to internal DTOs.
- Resilience policies (timeouts, retries, circuit breakers).

**Rules**
- Implements interfaces from `App.Abstractions`.
- Vendor DTOs never escape this project.

### `App.Api`
**Purpose**
- Minimal API endpoints.
- DI configuration and cross-cutting concerns.
- Centralized validation filter and error contract mapping.

**Rules**
- Depends on `App.Contracts`, `App.Validation`, `App.Abstractions`, and `App.Integrations`.

### `App.Ui`
**Purpose**
- Blazor Web App UI.
- Uses DTOs from `App.Contracts`.
- Runs local validation for immediate feedback.

**Rules**
- Depends on `App.Contracts`, `App.Validation`.
- Should not depend on `App.Integrations`.

## Validation Architecture

### Local vs Remote Rules
Split rules into two modes to avoid UI dependency on remote services:
- **Local rules**: synchronous or deterministic checks that run in UI and API.
- **Remote rules**: async checks that require external lookups; run only on API submit.

**Implementation options**
- Tag rules using a custom rule set or a validation context flag.
- UI adapter passes a context flag to exclude remote rules.
- API uses full rule set by default.

### Error Codes and Property Paths
- Every rule must emit a stable error code.
- Property paths must align with DTO property names used in the UI.

## API Validation Integration
- Use a route-group filter or endpoint filter to:
  - Resolve `IValidator<T>` from DI.
  - Run `ValidateAsync`.
  - Produce a consistent ProblemDetails response with both messages and codes.

**Error contract**
- Standardize a single shape for validation failures, for example:
  - HTTP 400 with ProblemDetails
  - `errors`: dictionary of property path to messages
  - `errorCodes`: dictionary of property path to codes

## UI Validation Integration
- Use a FluentValidation adapter for Blazor forms.
- Use local rules for immediate feedback.
- On submit, call API and map server validation errors back to the form.

## External Integrations Strategy
- Treat all external APIs as unstable.
- Isolate vendor DTOs inside `App.Integrations`.
- Normalize external errors into internal error contracts.
- Enforce timeouts by default; retries only for safe/idempotent calls.

## Testing Strategy

### `App.Validation.Tests`
- One test class per validator.
- Assert error property paths and error codes.
- Mock `App.Abstractions` interfaces for async rules.

### `App.Api.Tests`
- In-memory hosting via `WebApplicationFactory`.
- Override integration interfaces with fakes.
- Validate error contract shape and codes for invalid inputs.

### `App.E2E.Tests`
- Small Playwright suite:
  - Key happy paths.
  - Key invalid input flows.
- Prefer role-based selectors for stability.

## Formatting and CI

### Formatting
- Use CSharpier with a dotnet tool manifest.
- CI gate uses `dotnet csharpier --check .`.

### CI Pipeline (Azure DevOps)
1. Format
2. Build (Release)
3. Validation unit tests
4. API integration tests
5. E2E tests (required or scheduled based on runtime)

## Maintainability and Governance
- Enforce dependency direction via conventions or architecture tests.
- Prefer vertical slices in API for feature organization.
- Add OpenAPI generation and optional diff checks for contract safety.

## Open Questions (with Proposed Solutions)

1. **Should UI and API be in a single host or separate deployments?**
   - Proposed solution: start with a single host for simplicity. Separate later if scaling or security boundaries require it.

2. **How to handle remote validation rules in the UI?**
   - Proposed solution: run only local rules in the UI, run full validation in the API, and map API errors back into the form.

3. **Where should async validation interfaces live?**
   - Proposed solution: place them in `App.Abstractions` so `App.Validation` can depend on them without dragging in integrations.

4. **What should the validation error response shape be?**
   - Proposed solution: ProblemDetails + `errors` + `errorCodes`, with stable property paths. Add API tests to lock the contract.

5. **Localization strategy for validation messages?**
   - Proposed solution: keep stable error codes now; defer localization to a later phase by mapping codes to localized strings in UI.

6. **Resilience policy defaults for external APIs?**
   - Proposed solution: standard timeouts and limited retries for idempotent calls, with circuit breaker for unstable providers.

