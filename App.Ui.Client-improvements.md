# App.Ui.Client Improvements Review

Date: 2026-01-27
Scope: `src/App.Ui.Client` and related tests in `tests/App.Ui.Client.Tests` and `tests/App.E2E.Tests`
Test status: `dotnet test --solution FormValidationTest.sln` passed locally (123/123).

## Findings With Impact, Proposed Solution, and Status

| Area | Impactfulness Review | Issue | Evidence | Proposed Solution | Status |
| --- | --- | --- | --- | --- | --- |
| Choice group IDs (radio/checkbox) | High: can break label->input association, produce invalid/duplicate IDs, and make tests brittle when `option.Value.ToString()` contains spaces/symbols or is non-unique. | DOM IDs are derived from `option.Value`. | `src/App.Ui.Client/Components/Forms/FormCheckboxGroupField.razor:9`, `src/App.Ui.Client/Components/Forms/FormRadioGroupField.razor:9` | Use index-based IDs (e.g., `${Id}-opt-${index}`) for DOM wiring. Add a stable `data-option-value` attribute for testing/diagnostics. | DONE |
| Missing/empty `Id` handling | High: empty IDs degrade accessibility and can cause unintended radio-grouping collisions via `Name` fallback. | Components assume caller always supplies a valid `Id`. | `src/App.Ui.Client/Components/Forms/FormField.razor:24`, `src/App.Ui.Client/Components/Forms/FormRadioGroupField.razor:134` | Add safe fallback IDs when `Id` is blank: prefer field name from `ValueExpression` (when provided) plus a suffix; otherwise generate a unique component-scoped ID. | DONE |
| Prefill async race | Medium: older responses can win the race and overwrite newer user input/prefill, despite cancellation. | No “still-current lookup?” guard before applying results. | `src/App.Ui.Client/Pages/PrefillIntegrationDemo.razor:177`, `src/App.Ui.Client/Pages/PrefillIntegrationDemo.razor:211` | Capture the lookup key and compare before applying (`if (!string.Equals(_lastLookupName, lookupName, OrdinalIgnoreCase)) return;`). Also cancel/cleanup CTS on dispose. | DONE |
| `EditContext.OnFieldChanged` lifecycle | Medium: low risk for pages, but not detaching handlers is a leak/robustness smell and makes reuse/disposal less safe. | Handlers are attached but never detached. | `src/App.Ui.Client/Pages/SampleFormValidationWasm.razor:63`, `src/App.Ui.Client/Pages/ValidationExamples.razor:273`, `src/App.Ui.Client/Pages/TabbedForm.razor:209`, `src/App.Ui.Client/Pages/ComplexForm.razor:135`, `src/App.Ui.Client/Pages/PrefillIntegrationDemo.razor:137` | Store the handler delegate and detach it in `Dispose()` (implement `IDisposable`). | DONE |
| Enum select placeholder/nullability | Medium: component assumes an enum sentinel exists (e.g., `Unknown/None`). Without it, UX and validation can be misleading. | No placeholder option or nullable enum path. | `src/App.Ui.Client/Components/Forms/FormSelectEnumField.razor:11` | Consider adding optional placeholder support and/or a nullable enum mode. If keeping sentinel-only, document the requirement and add a guard/test. | DONE |
| bUnit coverage for validation wiring | Medium: manual `NotifyFieldChanged(...)` logic isn’t verified, so regressions can slip in. | Tests do not use a real `EditContext` + validator for field-changed behavior. | `tests/App.Ui.Client.Tests/FormComponentsTests.cs:154`, `tests/App.Ui.Client.Tests/FormComponentsTests.cs:184` | Add harness tests that render inside `EditForm` with a validator and assert validation state updates when toggling options and editing “Other”. | DONE |
| Test brittleness around IDs | Medium: current tests couple to ID construction details and will fight improvements to ID robustness. | Tests directly query IDs that embed option values. | `tests/App.Ui.Client.Tests/FormComponentsTests.cs:169`, `tests/App.Ui.Client.Tests/ValidationExamplesPageTests.cs:49` | Shift tests toward labels/roles and/or `data-*` attributes rather than value-derived DOM IDs. | DONE |
| E2E negative/edge-case coverage | Medium: mainly happy-path. Important regressions around validation gating and async behavior may not be caught. | No explicit tests for tab validation gating, stale prefill responses, or recovery flows. | `tests/App.E2E.Tests/TabbedFormE2ETests.cs:27`, `tests/App.E2E.Tests/PrefillIntegrationDemoE2ETests.cs:27` | Add a few high-value negative tests: cannot submit with missing required fields; switching tabs does not hide required errors silently; typing a new name cancels/ignores older prefill responses. | DONE |

## Suggested Implementation Order (High ROI)

1. Fix choice group ID generation and add `data-option-value` attributes.
2. Add safe `Id` fallback behavior across form fields/groups.
3. Guard against stale prefill responses and dispose the CTS/handlers.
4. Update bUnit tests to exercise validation wiring and remove ID brittleness.
5. Add a small set of negative/edge-case E2E tests.
