# App.Validation

Shared FluentValidation rules and validation utilities for the solution.

## Contents

- `SampleFormValidator` and `CustomerIntakeFormValidator` contain form-specific rules.
- `FinnishSsn` provides parsing and validation for Finnish personal identity codes (HETU).
- `FinnishBusinessIds` provides validation and checksum-based generation for Finnish Business IDs and VAT numbers.
- `IbanValidation` validates IBANs via the `IbanNet` library, and `IbanNet.FluentValidation` is available for rule-based IBAN validation.

## Validation Examples (Optional + Required)

A dedicated demo page exercises common field types and shows both optional and required variants:

- Route: `/validation-examples` in `src/App.Ui.Client/Pages/ValidationExamples.razor`
- Shared contract: `src/App.Contracts/ValidationExamplesForm.cs`
- Shared validator: `src/App.Validation/ValidationExamplesFormValidator.cs`

### Optional vs Required Pattern

For most fields, the validator follows this structure:

- Optional: validate only when a value is provided via `When(x => !string.IsNullOrWhiteSpace(...))`
- Required: use `NotEmpty()` for presence, then apply the same validity rule behind a `When(...)`

### Finnish Locale Number Parsing

Finnish-formatted decimal, currency, and percentage inputs are modeled as strings and parsed centrally:

- Parsing helper: `src/App.Validation/FinnishNumberParsing.cs`
- Accepted inputs include: `1 234,56`, `1 234,56 €`, `EUR 1 234,56`, and `12,5%`

### Multiple Choice Components

Reusable choice components live in the client UI project:

- Option type: `src/App.Ui.Client/Components/Forms/ChoiceOption.cs`
- Single choice (radio): `src/App.Ui.Client/Components/Forms/FormRadioGroupField.razor`
- Multiple choice (checkboxes): `src/App.Ui.Client/Components/Forms/FormCheckboxGroupField.razor`

### Tests to Use as Templates

- Validator tests: `tests/App.Validation.Tests/ValidationExamplesFormValidatorTests.cs`
- Parsing tests: `tests/App.Validation.Tests/FinnishNumberParsingTests.cs`
- Component tests: `tests/App.Ui.Client.Tests/FormComponentsTests.cs`
- Page coverage: `tests/App.Ui.Client.Tests/ValidationExamplesPageTests.cs`

## Finnish SSN Origin & Attribution

The `FinnishSsn` implementation and its unit tests were ported from a TypeScript
validator and tests in the following project (MIT licensed):

```text
https://github.com/orangitfi/finnish-ssn-validator
```

Notable adaptations:

- The SSN **generation** helpers were intentionally omitted.
- `DateOnly` is used for deterministic age and date-of-birth handling in tests.

## Finnish Business ID Origin & Attribution

The `FinnishBusinessIds` implementation and its unit tests were ported from a
TypeScript validator and tests in the following project:

```text
https://github.com/vkomulai/finnish-business-ids
```

## IBAN Validation Origin & Attribution

IBAN validation is implemented using the `IbanNet` and `IbanNet.FluentValidation`
NuGet packages:

```text
https://www.nuget.org/packages/IbanNet/
https://www.nuget.org/packages/IbanNet.FluentValidation/
```
