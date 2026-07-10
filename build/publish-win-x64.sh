#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$ROOT/artifacts/win-x64"
mkdir -p "$OUT"
dotnet publish "$ROOT/src/PathLengthChecker/PathLengthChecker.csproj" -c Release -r win-x64 --self-contained false -o "$OUT/cli"
# WPF GUI requires Windows to publish.
if [[ "$(uname -s)" == "Linux" ]] || [[ "$(uname -s)" == "Darwin" ]]; then
  echo "Skipping GUI publish on $(uname -s); build PathLengthCheckerGUI on Windows."
else
  dotnet publish "$ROOT/src/PathLengthCheckerGUI/PathLengthCheckerGUI.csproj" -c Release -r win-x64 --self-contained false -o "$OUT/gui"
fi
echo "Published to $OUT"
