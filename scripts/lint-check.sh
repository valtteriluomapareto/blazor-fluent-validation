#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root_dir"

echo "==> dotnet tool restore"
dotnet tool restore

echo "==> dotnet csharpier check ."
dotnet csharpier check .

echo "==> dotnet format analyzers FormValidationTest.sln --no-restore --verify-no-changes $*"
dotnet format analyzers FormValidationTest.sln --no-restore --verify-no-changes "$@"
