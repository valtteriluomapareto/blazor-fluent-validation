# FormValidationTest

Blazor Web App + Minimal API + FluentValidation solution organized for a validation-first workflow.

## Prerequisites
- .NET SDK 10.0.x
- Node.js (for Tailwind CSS)

## Solution Layout
- Host: `src/App.Host` — Single entry point that runs UI + API together (recommended for demos).
- UI host: `src/App.Ui` — Blazor Web App host and server-rendered shell. References the client assembly for interactive pages.
- UI client: `src/App.Ui.Client` — Blazor WebAssembly client assembly containing shared pages, form components, and client services.
- API: `src/App.Api` — Minimal API backend. Runs full validation (local + server rules), returns standardized error contracts.
- Shared:
  - `src/App.Contracts` — DTOs and shared response/error contracts used by UI and API.
  - `src/App.Abstractions` — Interfaces used by validation or API orchestration without coupling to integrations (placeholder for future async checks).
  - `src/App.Validation` — FluentValidation rules for contracts (local + server rule sets).
  - `src/App.Integrations` — External API adapters and vendor DTOs (placeholder for future integrations).
- Tests:
  - `tests/App.Validation.Tests` — Unit tests for validators and error codes.
  - `tests/App.Api.Tests` — API tests validating response shapes and validation behavior.
  - `tests/App.Ui.Client.Tests` — bUnit component tests for the WASM client.
  - `tests/App.E2E.Tests` — Playwright end-to-end tests that boot `App.Host`.

## Commands (from repo root)

Restore dependencies:
```bash
dotnet restore
```

Build all projects:
```bash
dotnet build FormValidationTest.sln -c Release
```

Run all tests (includes slower E2E):
```bash
dotnet test --solution FormValidationTest.sln -c Release
```

Run fast tests only (skips E2E):
```bash
./scripts/test-fast.sh -c Release
```
This runs validation, API, and bUnit tests:
- `tests/App.Validation.Tests/App.Validation.Tests.csproj`
- `tests/App.Api.Tests/App.Api.Tests.csproj`
- `tests/App.Ui.Client.Tests/App.Ui.Client.Tests.csproj`

Run E2E tests separately:
```bash
dotnet test --project tests/App.E2E.Tests/App.E2E.Tests.csproj -c Release
```

## Playwright E2E setup and usage

Official docs: `https://playwright.dev/dotnet/docs/intro`

1) Build the E2E project (generates the Playwright script):
```bash
dotnet build tests/App.E2E.Tests/App.E2E.Tests.csproj
```

2) Install Playwright browsers:
```bash
pwsh tests/App.E2E.Tests/bin/Debug/net10.0/playwright.ps1 install
```

3) Run the E2E suite with line-friendly output:
```bash
dotnet test --project tests/App.E2E.Tests/App.E2E.Tests.csproj --logger "console;verbosity=detailed"
```

Useful E2E environment variables:
- `E2E_BASE_URL` — point tests at an already running host (skips auto-start).
- `E2E_APP_URL` — override the auto-start URL (default `http://127.0.0.1:5010`).
- `E2E_HEADFUL=1` — run the browser headful for debugging.

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

Lint check (formatting + analyzers):
```bash
./scripts/lint-check.sh
```

Lint fix (formatting + analyzers where possible):
```bash
./scripts/lint-fix.sh
```

## Package update checks (dotnet-outdated)

Install/restore local tools:
```bash
dotnet tool restore
```

Check for outdated NuGet packages:
```bash
dotnet tool run dotnet-outdated -f
```

Upgrade outdated packages in-place:
```bash
dotnet tool run dotnet-outdated -f -u
```

## IDE setup (CSharpier)

VS Code
- Install the recommended extension (see `.vscode/extensions.json`).
- Run the `CSharpier: Check` task to verify formatting.

Rider / Visual Studio
- Install the CSharpier plugin/extension and enable format-on-save or run `dotnet csharpier check .`.

Run UI + API in a single host (recommended):
```bash
dotnet run --project src/App.Host
```
When running the host from Rider with Hot Reload enabled, changes under `src/App.Ui` and `src/App.Ui.Client` are watched by the host project for live updates.

Watch the host with hot reload:
```bash
dotnet watch --project src/App.Host
```

Run the UI only (requires API running at `Api:BaseUrl`):
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
This runs `dotnet watch` on the host plus the WASM client for hot reload and opens the UI browser based on `launchSettings.json`.

## Tailwind CSS

Initial setup (optional, speeds up first build):
```bash
cd src/App.Ui
npm install
```

Build the CSS manually:
```bash
cd src/App.Ui
npm run tailwind:build
```

Watch for changes during development:
```bash
cd src/App.Ui
npm run tailwind:watch
```

The UI build runs Tailwind automatically and will install npm dependencies if missing.
The generated file lives at `src/App.Ui/wwwroot/styles.css` and is intentionally not committed.

## Git commit guidance (Tailwind)

Commit:
- `src/App.Ui/tailwind.config.js`
- `src/App.Ui/Styles/tailwind.css`
- `src/App.Ui/package.json` and `src/App.Ui/package-lock.json`
- Any Razor/CSS/HTML updates that use Tailwind

Do not commit:
- `node_modules/`
- `src/App.Ui/wwwroot/styles.css` (generated)

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

If Tailwind styles are missing:
1) Ensure Node.js is installed.
2) Run:
```bash
cd src/App.Ui
npm install
npm run tailwind:build
```
