#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root_dir"

projects=(
  "tests/App.Validation.Tests/App.Validation.Tests.csproj"
  "tests/App.Api.Tests/App.Api.Tests.csproj"
  "tests/App.Ui.Client.Tests/App.Ui.Client.Tests.csproj"
)

for project in "${projects[@]}"; do
  echo "==> dotnet test --project $project"
  dotnet test --project "$project" "$@"
done
