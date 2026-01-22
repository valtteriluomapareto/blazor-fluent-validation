# Repository Guidelines

## Project Structure & Module Organization
- `FormValidationTest.sln` is the solution entry point; source lives under `src/`.
- UI project: `src/App.Ui/` (Blazor Web App).
- API project: `src/App.Api/` (Minimal API).
- Shared libraries:
  - `src/App.Contracts/` for shared DTOs.
  - `src/App.Abstractions/` for shared interfaces.
  - `src/App.Validation/` for FluentValidation rules.
  - `src/App.Integrations/` for external API adapters.
- UI pages are in `src/App.Ui/Components/Pages/` (`*.razor`).
- Shared UI layout is in `src/App.Ui/Components/Layout/`.
- Static assets are in `src/App.Ui/wwwroot/`.
- App configuration:
  - UI: `src/App.Ui/appsettings.json` (+ `appsettings.Development.json`).
  - API: `src/App.Api/appsettings.json` (+ `appsettings.Development.json`).

## Build, Test, and Development Commands
Use the .NET 10 SDK (projects target `net10.0`).
- `dotnet restore` — restore NuGet dependencies.
- `dotnet build FormValidationTest.sln` — build the full solution.
- `dotnet run --project src/App.Ui` — run the UI.
- `dotnet run --project src/App.Api` — run the API.
- `dotnet watch --project src/App.Ui` — run UI with hot reload.
- `dotnet publish src/App.Ui/App.Ui.csproj -c Release` — produce UI deployable output.
- `docker build -t formvalidationtest-ui -f src/App.Ui/Dockerfile .` — build the UI container image.
- `dotnet tool restore` — restore local dotnet tools (CSharpier).
- `dotnet csharpier .` — format the repo.
- `dotnet csharpier --check .` — verify formatting in CI.

## Coding Style & Naming Conventions
- Indentation: 4 spaces in C# and Razor.
- Components and classes: PascalCase; files match component/class name (e.g., `Counter.razor`).
- Locals and parameters: camelCase.
- Nullable reference types are enabled; prefer explicit null checks and `?` where appropriate.
- Formatting is enforced with CSharpier; run `dotnet csharpier .` before committing.

## Testing Guidelines
There are no test projects in this repository yet. If you add tests:
- Place them in a sibling project under `src/` or `tests/` (e.g., `tests/App.Validation.Tests/`).
- Run `dotnet test` — executes all tests in the solution.
Name test files to mirror the unit under test (e.g., `WeatherForecastTests.cs`).

## Commit & Pull Request Guidelines
- Commit history is minimal and uses short sentence case (e.g., “Initial scaffolding”). Keep subjects concise and action-oriented.
- PRs should include:
  - A brief summary of the change and rationale.
  - Testing notes (`dotnet run`, `dotnet test`, or “not run”).
  - Screenshots or GIFs for UI changes in `src/App.Ui/Components/`.

## Configuration & Security Tips
- Keep secrets out of `appsettings*.json`; use environment variables or user secrets for local development.
- Avoid committing generated content (`bin/`, `obj/`) per `.gitignore`.
