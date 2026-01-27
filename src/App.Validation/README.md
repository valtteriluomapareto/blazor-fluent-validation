# App.Validation

Shared FluentValidation rules and validation utilities for the solution.

## Contents

- `SampleFormValidator` and `CustomerIntakeFormValidator` contain form-specific rules.
- `FinnishSsn` provides parsing and validation for Finnish personal identity codes (HETU).

## Finnish SSN Origin & Attribution

The `FinnishSsn` implementation and its unit tests were ported from a TypeScript
validator and tests in the following project (MIT licensed):

```text
https://github.com/orangitfi/finnish-ssn-validator
```

Notable adaptations:

- The SSN **generation** helpers were intentionally omitted.
- `DateOnly` is used for deterministic age and date-of-birth handling in tests.

