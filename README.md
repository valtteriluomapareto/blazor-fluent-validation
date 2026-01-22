# FormValidationTest

Blazor Web App + Minimal API + FluentValidation solution organized for a validation-first workflow.

## Prerequisites
- .NET SDK 10.0.x

## Solution Layout
- UI: `src/App.Ui` — Blazor Web App frontend. Uses contracts + validators for local form feedback and submits to the API.
- API: `src/App.Api` — Minimal API backend. Runs full validation (local + server rules), returns standardized error contracts.
- Shared:
  - `src/App.Contracts` — DTOs and shared response/error contracts used by UI and API.
  - `src/App.Abstractions` — Interfaces used by validation or API orchestration without coupling to integrations (placeholder for future async checks).
  - `src/App.Validation` — FluentValidation rules for contracts (local + server rule sets).
  - `src/App.Integrations` — External API adapters and vendor DTOs (placeholder for future integrations).
- Tests:
  - `tests/App.Validation.Tests` — Unit tests for validators and error codes.
  - `tests/App.Api.Tests` — API tests validating response shapes and validation behavior.

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

Run UI + API together:
```bash
./scripts/dev.sh
```
This uses `dotnet watch` for both projects and will open the UI browser based on `launchSettings.json`.

## Troubleshooting

Styles missing or CSS isolation not applying (scoped CSS selectors like `.page[b-...]` don't match the rendered HTML):
1) Stop the running app.
2) Delete build artifacts (per project):
   - `src/App.Ui/bin`
   - `src/App.Ui/obj`
3) Rebuild and run the UI again:
```bash
dotnet build src/App.Ui/App.Ui.csproj
dotnet run --project src/App.Ui
```
This clears stale generated scoped CSS and static web asset manifests that can get out of sync.
