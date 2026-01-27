#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root_dir"

echo "==> dotnet tool restore"
dotnet tool restore

echo "==> dotnet csharpier format ."
dotnet csharpier format .

echo "==> dotnet format analyzers FormValidationTest.sln --no-restore $*"
dotnet format analyzers FormValidationTest.sln --no-restore "$@"
