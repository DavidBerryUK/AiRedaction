#!/usr/bin/env bash
# Starts the Redaction Demo web app and opens it in your browser.
# Usage: ./run-web.sh [--port 5199] [--input <folder>] [--output <folder>] [--config <file>] [--no-open]
set -euo pipefail
cd "$(dirname "$0")"

OPEN=1; ARGS=()
for a in "$@"; do if [ "$a" = "--no-open" ]; then OPEN=0; else ARGS+=("$a"); fi; done
PORT=5199
for i in "${!ARGS[@]}"; do if [ "${ARGS[$i]}" = "--port" ]; then PORT="${ARGS[$((i + 1))]:-5199}"; fi; done

say() { printf '\033[1m%s\033[0m\n' "$*"; }
fail() { printf '\033[31m%s\033[0m\n' "$*" >&2; exit 1; }

command -v dotnet >/dev/null || fail "The .NET 10 SDK is not installed (https://dotnet.microsoft.com/download)."
curl -fs --max-time 3 http://localhost:11434/api/tags >/dev/null \
  || fail "Ollama is not running. Start the Ollama app (or run: ollama serve) and try again."
if lsof -iTCP:"$PORT" -sTCP:LISTEN >/dev/null 2>&1; then fail "Port $PORT is already in use. Stop the other copy, or run: ./run-web.sh --port 5200"; fi

say "Building…"
dotnet build src/AiDocumentRedactor.App.Web -v q --nologo 2>&1 | grep -E " error " | sort -u || true
[ -f src/AiDocumentRedactor.App.Web/bin/Debug/net10.0/AiDocumentRedactor.App.Web.dll ] || fail "The build failed. Run: dotnet build"

say "Starting the Redaction Demo (Ctrl+C to stop)…"
export ASPNETCORE_ENVIRONMENT=Development
opened=0
dotnet src/AiDocumentRedactor.App.Web/bin/Debug/net10.0/AiDocumentRedactor.App.Web.dll --config redactor.config.json --port "$PORT" ${ARGS[@]+"${ARGS[@]}"} 2>&1 \
  | while IFS= read -r line; do
      echo "$line"
      if [ "$opened" = 0 ] && [[ "$line" =~ (http://127\.0\.0\.1:[0-9]+/\?t=[0-9a-f]+) ]]; then
        opened=1
        if [ "$OPEN" = 1 ]; then (open "${BASH_REMATCH[1]}" 2>/dev/null || xdg-open "${BASH_REMATCH[1]}" 2>/dev/null || true); fi
      fi
    done
