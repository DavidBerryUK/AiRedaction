#!/usr/bin/env bash
# Runs the evaluation over the test corpus with the models switched on in redactor.config.json,
# then opens the Markdown report. It can take a long time.
# Usage: ./run-eval.sh [--models a,b] [--only text/] [--no-write] [--show-text] [--out file.md] [--no-open]
set -euo pipefail
cd "$(dirname "$0")"

OPEN=1; ARGS=()
for a in "$@"; do if [ "$a" = "--no-open" ]; then OPEN=0; else ARGS+=("$a"); fi; done

say() { printf '\033[1m%s\033[0m\n' "$*"; }
fail() { printf '\033[31m%s\033[0m\n' "$*" >&2; exit 1; }

command -v dotnet >/dev/null || fail "The .NET 10 SDK is not installed (https://dotnet.microsoft.com/download)."
curl -fs --max-time 3 http://localhost:11434/api/tags >/dev/null \
  || fail "Ollama is not running. Start the Ollama app (or run: ollama serve) and try again."

say "Building…"
dotnet build src/AiDocumentRedactor.Eval -v q --nologo 2>&1 | grep -E " error " | sort -u || true
[ -f src/AiDocumentRedactor.Eval/bin/Debug/net10.0/AiDocumentRedactor.Eval.dll ] || fail "The build failed. Run: dotnet build"

say "Running the evaluation (this can take a long time; Ctrl+C stops it, and the report so far is kept)…"
LOG="$(mktemp)"
status=0
dotnet src/AiDocumentRedactor.Eval/bin/Debug/net10.0/AiDocumentRedactor.Eval.dll ${ARGS[@]+"${ARGS[@]}"} 2>&1 | tee "$LOG" || status=$?
REPORT="$(grep -E '^Report: ' "$LOG" | tail -1 | sed 's/^Report: //')"
rm -f "$LOG"
if [ -n "$REPORT" ] && [ -f "$REPORT" ]; then
  say "Report: $REPORT"
  if [ "$OPEN" = 1 ]; then (open "$REPORT" 2>/dev/null || xdg-open "$REPORT" 2>/dev/null || true); fi
elif [ "$status" -ne 0 ]; then
  fail "No report was written (see the messages above)."
fi
exit "$status"
