# FormValidationTest

Blazor Web App + Minimal API + FluentValidation solution organized for a validation-first workflow.

## Prerequisites
- .NET SDK 10.0.x

## Solution Layout
- UI: `src/App.Ui`
- API: `src/App.Api`
- Shared:
  - `src/App.Contracts`
  - `src/App.Abstractions`
  - `src/App.Validation`
  - `src/App.Integrations`
- Tests:
  - `tests/App.Validation.Tests`

## Commands (from repo root)

Restore dependencies:
```bash
dotnet restore
```

Build all projects:
```bash
dotnet build FormValidationTest.sln -c Release
```

Run tests:
```bash
dotnet test FormValidationTest.sln -c Release
```

Restore local tools (CSharpier):
```bash
dotnet tool restore
```

Format code:
```bash
dotnet csharpier format .
```

Check formatting (CI):
```bash
dotnet csharpier check .
```

Run the UI:
```bash
dotnet run --project src/App.Ui
```

Run the API:
```bash
dotnet run --project src/App.Api
```
