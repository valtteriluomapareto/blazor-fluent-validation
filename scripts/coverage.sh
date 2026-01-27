#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root_dir"

configuration="Release"
output_dir="$root_dir/TestResults/coverage"
include_e2e=0
generate_html=1
generate_xml=1

usage() {
  cat <<USAGE
Usage: ./scripts/coverage.sh [options]

Options:
  -c, --configuration <CONFIG>  Build configuration (default: Release)
  -o, --output <DIR>            Output directory (default: TestResults/coverage)
      --include-e2e             Include Playwright E2E tests in the run
      --html-only               Generate HTML only (no merged XML)
      --xml-only                Generate merged Cobertura XML only (no HTML)
      --no-html                 Skip HTML output
      --no-xml                  Skip merged Cobertura XML output
  -h, --help                    Show this help message
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -c|--configuration)
      configuration="$2"
      shift 2
      ;;
    -o|--output)
      output_dir="$2"
      shift 2
      ;;
    --include-e2e)
      include_e2e=1
      shift
      ;;
    --html-only)
      generate_html=1
      generate_xml=0
      shift
      ;;
    --xml-only)
      generate_xml=1
      generate_html=0
      shift
      ;;
    --no-html)
      generate_html=0
      shift
      ;;
    --no-xml)
      generate_xml=0
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      usage
      exit 1
      ;;
  esac
done

if [[ "$generate_html" -eq 0 && "$generate_xml" -eq 0 ]]; then
  echo "At least one output type must be enabled (HTML or XML)." >&2
  exit 1
fi

if [[ "$output_dir" != /* ]]; then
  output_dir="$root_dir/$output_dir"
fi

raw_dir="$output_dir/raw"
report_dir="$output_dir/report"

rm -rf "$output_dir"
mkdir -p "$raw_dir" "$report_dir"

echo "==> dotnet tool restore"
dotnet tool restore >/dev/null

projects=(
  "tests/App.Validation.Tests/App.Validation.Tests.csproj"
  "tests/App.Api.Tests/App.Api.Tests.csproj"
  "tests/App.Ui.Client.Tests/App.Ui.Client.Tests.csproj"
)

if [[ "$include_e2e" -eq 1 ]]; then
  projects+=("tests/App.E2E.Tests/App.E2E.Tests.csproj")
fi

for project in "${projects[@]}"; do
  project_name="$(basename "$project" .csproj)"
  coverage_file="$raw_dir/coverage.$project_name.cobertura.xml"

  echo "==> dotnet test --project $project -c $configuration -- --coverage --coverage-output-format cobertura --coverage-output $coverage_file"
  dotnet test --project "$project" -c "$configuration" -- \
    --coverage \
    --coverage-output-format cobertura \
    --coverage-output "$coverage_file"
done

shopt -s nullglob
coverage_files=("$raw_dir"/coverage.*.cobertura.xml)

if [[ ${#coverage_files[@]} -eq 0 ]]; then
  echo "No coverage files were produced." >&2
  exit 1
fi

report_types=("TextSummary")
if [[ "$generate_html" -eq 1 ]]; then
  report_types+=("Html")
fi
if [[ "$generate_xml" -eq 1 ]]; then
  report_types+=("Cobertura")
fi

reports_arg="$(IFS=';'; echo "${coverage_files[*]}")"
report_types_arg="$(IFS=';'; echo "${report_types[*]}")"

echo "==> dotnet tool run reportgenerator -reports:$reports_arg -targetdir:$report_dir -reporttypes:$report_types_arg"
dotnet tool run reportgenerator \
  "-reports:$reports_arg" \
  "-targetdir:$report_dir" \
  "-reporttypes:$report_types_arg" >/dev/null

if [[ -f "$report_dir/Summary.txt" ]]; then
  echo "==> Coverage summary"
  cat "$report_dir/Summary.txt"
fi

if [[ "$generate_xml" -eq 1 && -f "$report_dir/Cobertura.xml" ]]; then
  cp "$report_dir/Cobertura.xml" "$output_dir/coverage.cobertura.xml"
  echo "==> Merged Cobertura XML: $output_dir/coverage.cobertura.xml"
fi

if [[ "$generate_html" -eq 1 && -f "$report_dir/index.html" ]]; then
  echo "==> HTML report: $report_dir/index.html"
fi

echo "==> Raw coverage files: $raw_dir"
