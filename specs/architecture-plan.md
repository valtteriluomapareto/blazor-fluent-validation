# Architecture Plan (Validation-First, C#-Only Stack)

## Goals
- C#-only developer experience using Blazor (no SPA frameworks).
- Single source of truth for validation rules shared by UI and API.
- High confidence in changes through layered testing (validation unit tests, API integration tests, minimal E2E).
- No internal database; orchestration of external APIs with robust validation and error handling.

## Status (High Level)
Done
- Solution structure and project boundaries implemented.
- Validation-first flow: local rule sets in UI, full rule sets in API.
- Dual-mode UI (Server + WASM) for demo purposes via `App.Ui` + `App.Ui.Client`.
- `App.Host` runs UI + API together for demos and E2E tests.
- Mock integration in `App.Integrations` for async validation.
- ProblemDetails-style validation error contract wired into API.
- API tests lock the validation error contract and key rule sets.
- Playwright E2E suite is in place for key flows.
- UI component tests (`App.Ui.Client.Tests`) cover shared form components and pages.
- CI runs formatting/linting, non-E2E tests, and E2E tests.

In progress / remaining
- Localization strategy for validation messages.
- Resilience defaults for external API calls.
- Expand E2E coverage beyond the current key flows.

## Remaining Work: Task List
Use these as concrete work items with suggested priority and owner placeholders.

P0 (Next)
- Decide and document localization strategy for validation messages (error codes -> UI mapping). Owner: TBD.
- Add resilience defaults (timeouts + limited retries) for integrations and typed HTTP clients. Owner: TBD.

P1 (Soon)
- Expand E2E scenarios around server-returned validation errors and multi-step flows. Owner: TBD.

P2 (Later)
- Enforce dependency direction via architecture tests or conventions in CI. Owner: TBD.

## Key Principles
- Separation by responsibility: contracts, validation, integrations, API, UI.
- Explicit dependency direction; no cycles.
- Validation rules are stable, testable, and deterministic.
- External integrations are isolated behind adapters and normalized error contracts.

## Solution Structure (Projects)

### `App.Host`
**Purpose**
- Single-host entry point that runs UI + API together for demos and local development.
- Wires services and endpoints via `App.Api` extension methods.

**Rules**
- References `App.Ui`, `App.Ui.Client`, and `App.Api`.

**Status**
- Implemented.

### `App.Contracts`
**Purpose**
- Shared request/response DTOs.
- Shared enums/value types.
- Shared error contract types.

**Rules**
- No dependency on UI/API/Integration projects.
- No vendor DTOs.

**Status**
- Implemented.

### `App.Abstractions` (recommended)
**Purpose**
- Small interfaces required by validation or API orchestration (e.g., lookup interfaces).
- Shared by `App.Validation` and `App.Api` without dragging UI concerns into integrations.

**Rules**
- No references to integrations or infrastructure.

**Status**
- Implemented.

### `App.Validation`
**Purpose**
- FluentValidation validators for `App.Contracts` DTOs.
- Reusable rule helpers and error code conventions.

**Async Validation**
- Validators may depend on narrow interfaces from `App.Abstractions` for async checks.

**Rules**
- Depends on `App.Contracts` and `App.Abstractions`.
- No references to `App.Api` or `App.Ui`.

**Status**
- Implemented.

### `App.Integrations` (or `App.Infrastructure`)
**Purpose**
- HTTP clients and adapters for external APIs.
- Vendor DTOs and mapping to internal DTOs.
- Resilience policies (timeouts, retries, circuit breakers).

**Rules**
- Implements interfaces from `App.Abstractions`.
- Vendor DTOs never escape this project.

**Status**
- Implemented with mock lookup; resilience policies pending.

### `App.Api`
**Purpose**
- Minimal API endpoints.
- DI configuration and cross-cutting concerns (exposed via module extensions).
- Centralized validation filter and error contract mapping.

**Rules**
- Depends on `App.Contracts`, `App.Validation`, `App.Abstractions`, and `App.Integrations`.

**Status**
- Implemented.

### `App.Ui`
**Purpose**
- Blazor Web App host (server-rendered shell + static assets).
- References `App.Ui.Client` for shared interactive pages and components.
- Wires validators and HTTP clients for interactive server rendering.

**Rules**
- Depends on `App.Contracts`, `App.Validation`, and `App.Ui.Client`.
- Should not depend on `App.Integrations`.

**Status**
- Implemented as the server host for dual-mode rendering.

### `App.Ui.Client`
**Purpose**
- Blazor WebAssembly client assembly containing shared pages, form components, and client-only services.
- Houses the reusable form field components and most validation demos.

**Rules**
- Depends on `App.Contracts`, `App.Validation`, and `App.Abstractions`.
- Should not depend on `App.Integrations`.

**Status**
- Implemented and covered by `App.Ui.Client.Tests`.

## Validation Architecture

### Local vs Remote Rules
Split rules into two modes to avoid UI dependency on remote services:
- **Local rules**: synchronous or deterministic checks that run in UI and API.
- **Remote rules**: async checks that require external lookups; run only on API submit.

**Implementation options**
- Tag rules using rule sets (implemented as `"Local"` and `"Server"`).
- UI runs `RuleSets="Local"` with Blazilla `AsyncMode="true"` to support a single validator.
- API uses both rule sets by default.

**Status**
- Implemented.

### Error Codes and Property Paths
- Every rule must emit a stable error code.
- Property paths must align with DTO property names used in the UI.

**Status**
- Implemented and covered by validation + API tests.

## API Validation Integration
- Use a route-group filter or endpoint filter to:
  - Resolve `IValidator<T>` from DI.
  - Run `ValidateAsync`.
  - Produce a consistent error response with both messages and codes.

**Error contract**
- Standardize a single shape for validation failures, for example:
  - HTTP 400 with `ValidationErrorResponse`
  - `errors`: dictionary of property path to messages
  - `errorCodes`: dictionary of property path to codes

**Status**
- Contract implemented and locked down with `App.Api.Tests`.

## UI Validation Integration
- Use a FluentValidation adapter for Blazor forms.
- Use local rules for immediate feedback in `App.Ui.Client` pages.
- On submit, call API and map server validation errors back to the form.

**Status**
- Implemented and reinforced with `App.Ui.Client.Tests` and E2E coverage.

## External Integrations Strategy
- Treat all external APIs as unstable.
- Isolate vendor DTOs inside `App.Integrations`.
- Normalize external errors into internal error contracts.
- Enforce timeouts by default; retries only for safe/idempotent calls.
- Current demo uses a mock lookup integration for "used names."

**Status**
- Mock integration implemented; resilience policies pending.

## Testing Strategy

### `App.Validation.Tests`
- One test class per validator.
- Assert error property paths and error codes.
- Mock `App.Abstractions` interfaces for async rules.

**Status**
- Implemented for current validators.

### `App.Api.Tests`
- In-memory hosting via `WebApplicationFactory`.
- Override integration interfaces with fakes.
- Validate error contract shape and codes for invalid inputs.

**Status**
- Implemented for validation error contracts and key endpoints.

### `App.Ui.Client.Tests`
- bUnit tests for shared form components and key validation pages.
- Focus on client-side validation UX and error surfacing behavior.

**Status**
- Implemented.

### `App.E2E.Tests`
- Small Playwright suite:
  - Key happy paths.
  - Key invalid input flows.
- Prefer role-based selectors for stability.

**Status**
- Implemented with `App.Host` fixture; expand coverage over time.

## Formatting and CI

### Formatting
- Use CSharpier with a dotnet tool manifest.
- CI gate uses `dotnet csharpier --check .`.

**Status**
- Implemented.

### CI Pipeline (Azure DevOps)
1. Format
2. Restore + lint check script
3. Build (Release)
4. Non-E2E tests via `scripts/test-fast.sh`
5. E2E tests via `tests/App.E2E.Tests`

**Status**
- Implemented, including E2E execution.

## Maintainability and Governance
- Enforce dependency direction via conventions or architecture tests.
- Prefer vertical slices in API for feature organization.
- Add OpenAPI generation and optional diff checks for contract safety.

## Open Questions (with Proposed Solutions)

1. **Should UI and API be in a single host or separate deployments?**
   - Implemented: `App.Host` runs UI + API in one process for demos. Separate deployments can be added later if needed.
   - Status: settled.

2. **How to handle remote validation rules in the UI?**
   - Implemented: UI runs `RuleSets="Local"` with Blazilla `AsyncMode="true"`, API runs all rules, and API errors are mapped back into the form.

3. **Where should async validation interfaces live?**
   - Implemented: async validation interfaces live in `App.Abstractions` so `App.Validation` can depend on them without dragging in integrations.
   - Status: settled.

4. **What should the validation error response shape be?**
   - Implemented: ProblemDetails-style response + `errors` + `errorCodes`, with stable property paths.
   - Status: contract implemented and covered by `App.Api.Tests`.

5. **Localization strategy for validation messages?**
   - Proposed solution: keep stable error codes now; defer localization to a later phase by mapping codes to localized strings in UI.

6. **Resilience policy defaults for external APIs?**
   - Proposed solution: standard timeouts and limited retries for idempotent calls, with circuit breaker for unstable providers.
