# Code Coverage Summary (xUnit v3 + MTP)

This summary is based on the latest coverage run from:
- Command: `./scripts/coverage.sh -c Release`
- Source: `TestResults/coverage/report/Summary.txt`
- Date: 2026-01-27

## Executive Summary
- Overall line coverage: **84.1%**
- Overall branch coverage: **72.8%**
- Strongest areas: `App.Contracts` (100%), `App.Validation` (95.5%), `App.Api` (91.9%)
- Main opportunity: `App.Ui.Client` (74.2%), with several key pages and components under-covered

## Coverage By Assembly (Line Coverage)
- `App.Contracts`: **100%**
- `App.Validation`: **95.5%**
- `App.Api`: **91.9%**
- `App.Integrations`: **91.5%**
- `App.Ui.Client`: **74.2%**

## Notable Gaps And Risks
The most important gaps are concentrated in the UI client, especially in user-critical pages and input components.

High-risk coverage gaps:
- `FormValidationTest.Client.Pages.TabbedForm`: **0%**
- `FormValidationTest.Client.Pages.ComplexForm`: **0%**
- `FormValidationTest.Client.Program`: **0%**

Low-to-medium coverage gaps:
- `FormDateField`: **56.2%**
- `FormTextAreaField`: **50%**
- `FormNumberField`: **64.2%**
- `FormDecimalField`: **67.8%**
- `FormFieldIdResolver`: **70.9%**
- `ValidationExamples` page: **71%**

These areas represent real user flows (pages) and parsing / binding behavior (form inputs), which are common sources of regressions.

## Proposed Improvements (Prioritized)
The goal is to raise UI confidence while keeping tests stable and fast.

### P0: Cover 0% UI Pages With bUnit
Focus on pages currently at 0%, using bUnit (not E2E) for speed and determinism.

Suggested additions:
- Add `TabbedFormPageTests` in `tests/App.Ui.Client.Tests/`.
- Add `ComplexFormPageTests` in `tests/App.Ui.Client.Tests/`.

For `TabbedForm`, target these scenarios:
- Initial render shows the first tab and expected fields.
- Navigation between tabs works via Next/Back.
- Submitting from the final tab with missing required fields surfaces validation summary messages.
- Fixing values clears validation messages and allows a success status message.

For `ComplexForm`, target these scenarios:
- Initial render includes all expected input types.
- A minimal valid submission path produces the expected success status.
- One invalid path per major input category produces a validation message.

### P1: Increase Coverage In Core Form Inputs
Form input components are the “engine room” of validation UX. Incrementally hardening them pays off.

Suggested additions (bUnit):
- `FormDateField`:
  - Valid ISO date input binds correctly.
  - Invalid date input results in validation error behavior.
- `FormNumberField`:
  - Numeric input updates model correctly.
  - Non-numeric input is handled safely.
- `FormDecimalField`:
  - Decimal input with and without separators binds correctly.
  - Edge cases (empty string, whitespace) behave consistently.
- `FormTextAreaField`:
  - Value binding works and updates the model.
  - Rows attribute is rendered as expected.

### P1: Add Targeted Tests For Field ID Resolution
`FormFieldIdResolver` is foundational for correct error placement.

Suggested additions:
- Add unit tests that cover:
  - Explicit `Id` is preferred when provided.
  - Expression-based IDs are stable and predictable.
  - Fallback IDs remain deterministic within a render.

### P2: Optional Coverage For Program And Local Services
These are low risk, but easy wins if a higher coverage bar is required.

Suggested additions:
- `FormValidationTest.Client.Program`:
  - Consider a very small smoke test around DI registration patterns via integration-like bUnit setup.
- `LocalUsedNameLookup`:
  - Add a trivial unit test asserting it returns an empty collection.

## Suggested Coverage Targets
Pragmatic targets that match the current architecture and test style:
- Keep `App.Validation`, `App.Api`, and `App.Contracts` above **90%** line coverage.
- Raise `App.Ui.Client` from **74.2%** to at least **85%** line coverage.
- Aim for overall branch coverage above **75%**.

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
