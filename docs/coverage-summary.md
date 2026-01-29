# Code Coverage Summary (xUnit v3 + MTP)

This summary is based on the latest coverage run from:
- Command: `./scripts/coverage.sh -c Release`
- Source: `TestResults/coverage/report/Summary.txt`
- Date: 2026-01-29

## Executive Summary
- Overall line coverage: **90.2%**
- Overall branch coverage: **78.8%**
- Strongest areas: `App.Contracts` (100%), `App.Validation` (95.5%), `App.Integrations` (91.5%)
- Main remaining opportunities: `App.Ui.Client` (87.4%) still has a few concentrated gaps

## Coverage By Assembly (Line Coverage)
- `App.Contracts`: **100%**
- `App.Validation`: **95.5%**
- `App.Api`: **85.9%**
- `App.Integrations`: **91.5%**
- `App.Ui.Client`: **87.4%**

## Notable Gaps And Risks
The most important gaps are concentrated in the UI client, especially in user-critical pages and input components.

High-risk coverage gaps:
- `FormValidationTest.Client.Program`: **0%**
- `FormValidationTest.Client.ClientProgram`: **45.2%**

Low-to-medium coverage gaps:
- `ValidationExamples` page: **71%**
- `PrefillIntegrationDemo` page: **79%**
- `LocalizedFluentValidator`: **87.6%**

Tabbed flows improved substantially:
- `FormValidationTest.Client.Pages.TabbedForm`: **100%**
- `FormValidationTest.Client.Pages.ComplexForm`: **100%**
- `FormValidationTest.Client.Services.LocalUsedNameLookup`: **100%**
- `FormValidationTest.Client.Components.Forms.FormDateField`: **94.7%**
- `FormValidationTest.Client.Components.Forms.FormDecimalField`: **96.7%**
- `FormValidationTest.Client.Components.Forms.FormFieldIdResolver`: **87%**
- `FormValidationTest.Client.Components.Forms.FormTabs`: **94.7%**
- `FormValidationTest.Client.Components.Forms.FormTextAreaField`: **94.8%**

The remaining gaps are mostly around DI/bootstrap (`Program` / `ClientProgram`) and a few input/validation plumbing components.

## Proposed Improvements (Prioritized)
The goal is to raise UI confidence while keeping tests stable and fast.

### P0: Close Remaining 0% Gaps
Focus first on the remaining 0% coverage areas.

Suggested additions:
- Treat `FormValidationTest.Client.Program` as a known 0% entry point in test runs.

Notes:
- `WebAssemblyHostBuilder.CreateDefault(...)` throws `PlatformNotSupportedException` in the unit test runner (it requires the WebAssembly JS runtime).
- As a mitigation, we extracted testable helpers in `ClientProgram` and added tests for culture, service registration, and base URL resolution.

### P1: Increase Coverage In Core Form Inputs And Plumbing
Form input components are the “engine room” of validation UX. Incrementally hardening them pays off.

Completed (bUnit):
- `LocalizedFluentValidator`:
  - Added tests for rule set override precedence and field-change validation updates.

### P2: Deepen Page And Validator Branch Coverage
These are already decent, but have room for more branch coverage.

Suggested additions:
- `ValidationExamples` page:
  - Add invalid-then-fix flows for a few representative inputs.
- `PrefillIntegrationDemo` page:
  - Add a submission flow test (not just field population and race behavior).

## Suggested Coverage Targets
Pragmatic targets that match the current architecture and test style:
- Keep `App.Validation`, `App.Api`, and `App.Contracts` above **90%** line coverage.
- Raise `App.Ui.Client` from **87.4%** to at least **90%** line coverage.
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
