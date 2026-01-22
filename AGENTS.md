# Repository Guidelines

## Project Structure & Module Organization
- `FormValidationTest.sln` is the solution entry point; the app lives in `FormValidationTest/`.
- `FormValidationTest/Program.cs` configures the ASP.NET Core + Blazor server pipeline.
- UI lives under `FormValidationTest/Components/`:
  - `Components/Pages/` for routable pages (`*.razor`).
  - `Components/Layout/` for shared layout and navigation.
- Static assets are in `FormValidationTest/wwwroot/` (CSS, Bootstrap, favicon).
- Configuration is in `FormValidationTest/appsettings.json` with local overrides in `FormValidationTest/appsettings.Development.json`.

## Build, Test, and Development Commands
Use the .NET 10 SDK (project targets `net10.0`).
- `dotnet restore` — restore NuGet dependencies.
- `dotnet build FormValidationTest/FormValidationTest.csproj` — build the app.
- `dotnet run --project FormValidationTest` — run locally.
- `dotnet watch --project FormValidationTest` — run with hot reload.
- `dotnet publish FormValidationTest/FormValidationTest.csproj -c Release` — produce deployable output.
- `docker build -t formvalidationtest -f FormValidationTest/Dockerfile .` — build the container image.

## Coding Style & Naming Conventions
- Indentation: 4 spaces in C# and Razor.
- Components and classes: PascalCase; files match component/class name (e.g., `Counter.razor`).
- Locals and parameters: camelCase.
- Nullable reference types are enabled; prefer explicit null checks and `?` where appropriate.
- No formatter is configured; follow existing patterns in `Program.cs` and `Components/`.

## Testing Guidelines
There is no test project in this repository yet. If you add tests, place them in a sibling project (e.g., `FormValidationTest.Tests/`) and run:
- `dotnet test` — executes all tests in the solution.
Name test files to mirror the unit under test (e.g., `WeatherForecastTests.cs`).

## Commit & Pull Request Guidelines
- Commit history is minimal and uses short sentence case (e.g., “Initial scaffolding”). Keep subjects concise and action-oriented.
- PRs should include:
  - A brief summary of the change and rationale.
  - Testing notes (`dotnet run`, `dotnet test`, or “not run”).
  - Screenshots or GIFs for UI changes in `Components/`.

## Configuration & Security Tips
- Keep secrets out of `appsettings*.json`; use environment variables or user secrets for local development.
- Avoid committing generated content (`bin/`, `obj/`) per `.gitignore`.
