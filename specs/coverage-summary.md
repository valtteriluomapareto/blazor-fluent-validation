# Code Coverage Summary (xUnit v3 + MTP)

This summary is based on the latest coverage run from:
- Command: `./scripts/coverage.sh -c Release`
- Source: `TestResults/coverage/report/Summary.txt`
- Date: 2026-01-27

## Executive Summary
- Overall line coverage: **90.4%**
- Overall branch coverage: **77.7%**
- Strongest areas: `App.Contracts` (100%), `App.Validation` (95.5%), `App.Api` (91.9%)
- Main remaining opportunities: `App.Ui.Client` (86.3%) still has a few concentrated gaps

## Coverage By Assembly (Line Coverage)
- `App.Contracts`: **100%**
- `App.Validation`: **95.5%**
- `App.Api`: **91.9%**
- `App.Integrations`: **91.5%**
- `App.Ui.Client`: **86.3%**

## Notable Gaps And Risks
The most important gaps are concentrated in the UI client, especially in user-critical pages and input components.

High-risk coverage gaps:
- `FormValidationTest.Client.Program`: **0%**
- `FormValidationTest.Client.ClientProgram`: **63.3%**

Low-to-medium coverage gaps:
- `FormDateField`: **68.7%**
- `FormFieldIdResolver`: **70.9%**
- `ValidationExamples` page: **71%**
- `PrefillIntegrationDemo` page: **78.2%**
- `FormDecimalField`: **78.5%**
- `LocalizedFluentValidator`: **81.7%**

Tabbed flows improved substantially:
- `FormValidationTest.Client.Pages.TabbedForm`: **100%**
- `FormValidationTest.Client.Pages.ComplexForm`: **100%**
- `FormValidationTest.Client.Services.LocalUsedNameLookup`: **100%**

The remaining gaps are mostly around DI/bootstrap (`Program` / `ClientProgram`) and a few input/validation plumbing components.

## Proposed Improvements (Prioritized)
The goal is to raise UI confidence while keeping tests stable and fast.

### P0: Close Remaining 0% Gaps
Focus first on the remaining 0% coverage areas.

Suggested additions:
- Consider excluding `FormValidationTest.Client.Program` from coverage reports.

Notes:
- `WebAssemblyHostBuilder.CreateDefault(...)` throws `PlatformNotSupportedException` in the unit test runner (it requires the WebAssembly JS runtime).
- As a mitigation, we extracted testable helpers in `ClientProgram` and added tests for culture, service registration, and base URL resolution.

### P1: Increase Coverage In Core Form Inputs And Plumbing
Form input components are the “engine room” of validation UX. Incrementally hardening them pays off.

Suggested additions (bUnit):
- `FormDateField`:
  - Valid ISO date input binds correctly.
  - Invalid date input results in validation error behavior.
- `FormDecimalField`:
  - Decimal input with and without separators binds correctly.
  - Edge cases (empty string, whitespace) behave consistently.

### P1: Add Targeted Tests For Field ID Resolution
`FormFieldIdResolver` is foundational for correct error placement.

Suggested additions:
- Add unit tests that cover:
  - Explicit `Id` is preferred when provided.
  - Expression-based IDs are stable and predictable.
  - Fallback IDs remain deterministic within a render.

### P2: Deepen Page And Validator Branch Coverage
These are already decent, but have room for more branch coverage.

Suggested additions:
- `ValidationExamples` page:
  - Add invalid-then-fix flows for a few representative inputs.
- `PrefillIntegrationDemo` page:
  - Add a submission flow test (not just field population and race behavior).
- `LocalizedFluentValidator`:
  - Add tests that exercise rule set selection and multi-field validation updates.

## Suggested Coverage Targets
Pragmatic targets that match the current architecture and test style:
- Keep `App.Validation`, `App.Api`, and `App.Contracts` above **90%** line coverage.
- Raise `App.Ui.Client` from **86.3%** to at least **90%** line coverage.
- Aim for overall branch coverage above **80%**.

## How To Reproduce And Inspect
Generate coverage (fast tests only):
```bash
dotnet tool restore
./scripts/coverage.sh -c Release
```

Useful outputs:
- Human-friendly HTML: `TestResults/coverage/report/index.html`
- LLM/CI-friendly XML: `TestResults/coverage/coverage.cobertura.xml`

Optional runs:
- Include E2E in coverage: `./scripts/coverage.sh --include-e2e`
- XML only: `./scripts/coverage.sh --xml-only`
- HTML only: `./scripts/coverage.sh --html-only`
