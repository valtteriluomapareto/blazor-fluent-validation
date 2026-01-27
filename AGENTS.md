# Repository Guidelines

## Project Structure & Module Organization
- `FormValidationTest.sln` is the solution entry point; source lives under `src/`.
- Host project: `src/App.Host/` (runs UI + API together; recommended for demos and E2E).
- UI host project: `src/App.Ui/` (Blazor Web App host).
- UI client project: `src/App.Ui.Client/` (Blazor WebAssembly client assembly with shared pages/components).
- API project: `src/App.Api/` (Minimal API).
- Shared libraries:
  - `src/App.Contracts/` for shared DTOs.
  - `src/App.Abstractions/` for shared interfaces.
  - `src/App.Validation/` for FluentValidation rules.
  - `src/App.Integrations/` for external API adapters.
- UI pages are primarily in `src/App.Ui.Client/Pages/` (`*.razor`).
- Shared form components are in `src/App.Ui.Client/Components/Forms/`.
- Shared UI layout and shell components are in `src/App.Ui/Components/Layout/`.
- Static assets are in `src/App.Ui/wwwroot/`.
- App configuration:
  - Host: `src/App.Host/appsettings.json`.
  - UI: `src/App.Ui/appsettings.json` (+ `appsettings.Development.json`).
  - API: `src/App.Api/appsettings.json` (+ `appsettings.Development.json`).

## Build, Test, and Development Commands
Use the .NET 10 SDK (projects target `net10.0`).
- `dotnet restore` — restore NuGet dependencies.
- `dotnet build FormValidationTest.sln` — build the full solution.
- `./scripts/lint-fix.sh` — always run this after making changes.
- `dotnet run --project src/App.Host` — run UI + API together (recommended).
- `dotnet run --project src/App.Ui` — run the UI.
- `dotnet run --project src/App.Api` — run the API.
- `dotnet watch --project src/App.Host` — run host with hot reload across UI + client.
- `dotnet watch --project src/App.Ui` — run UI host with hot reload.
- `./scripts/test-fast.sh -c Release` — run non-E2E tests (validation, API, bUnit).
- `dotnet test --project tests/App.E2E.Tests/App.E2E.Tests.csproj -c Release` — run Playwright E2E tests.
- `dotnet publish src/App.Ui/App.Ui.csproj -c Release` — produce UI deployable output.
- `docker build -t formvalidationtest-ui -f src/App.Ui/Dockerfile .` — build the UI container image.
- `dotnet tool restore` — restore local dotnet tools (CSharpier).
- `dotnet csharpier format .` — format the repo.
- `dotnet csharpier check .` — verify formatting in CI.

## Coding Style & Naming Conventions
- Indentation: 4 spaces in C# and Razor.
- Components and classes: PascalCase; files match component/class name (e.g., `Counter.razor`).
- Locals and parameters: camelCase.
- Nullable reference types are enabled; prefer explicit null checks and `?` where appropriate.
- Formatting is enforced with CSharpier; run `dotnet csharpier .` before committing.
- Always run `./scripts/lint-fix.sh` after making changes.

## Testing Guidelines
Existing test project:
- `tests/App.Validation.Tests` — validation unit tests.
- `tests/App.Api.Tests` — API integration/contract tests.
- `tests/App.Ui.Client.Tests` — bUnit component and page tests.
- `tests/App.E2E.Tests` — Playwright end-to-end tests (boots `App.Host`).

If you add more tests:
- Place them in a sibling project under `src/` or `tests/` (e.g., `tests/App.Api.Tests/`, `tests/App.E2E.Tests/`).
- Run `dotnet test` — executes all tests in the solution.
- Prefer `./scripts/test-fast.sh` during iteration, and run E2E separately when needed.
- Name test files to mirror the unit under test (e.g., `WeatherForecastTests.cs`).

## Commit & Pull Request Guidelines
- Commit history is minimal and uses short sentence case (e.g., “Initial scaffolding”). Keep subjects concise and action-oriented.
- PRs should include:
  - A brief summary of the change and rationale.
  - Testing notes (`dotnet run`, `dotnet test`, or “not run”).
  - Screenshots or GIFs for UI changes in `src/App.Ui.Client/Pages/` or `src/App.Ui.Client/Components/`.

## Configuration & Security Tips
- Keep secrets out of `appsettings*.json`; use environment variables or user secrets for local development.
- Avoid committing generated content (`bin/`, `obj/`) per `.gitignore`.
