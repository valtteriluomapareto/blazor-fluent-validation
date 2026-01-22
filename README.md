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
