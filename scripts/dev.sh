#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root_dir"

pids=()

cleanup() {
  for pid in "${pids[@]}"; do
    if kill -0 "$pid" 2>/dev/null; then
      kill "$pid"
    fi
  done
}

trap cleanup EXIT INT TERM

dotnet watch --project src/App.Host &
pids+=("$!")

dotnet watch --project src/App.Ui.Client &
pids+=("$!")

wait
