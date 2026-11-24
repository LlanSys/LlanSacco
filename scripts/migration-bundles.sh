#!/usr/bin/env bash
# Builds or runs one EF Core migration bundle per context listed in ops/migrations/contexts.txt.
#   build <SqlServer|PostgreSql> <output-dir>
#   run   <bundle-dir> <connection-string>
# The build step copies this script and the context list into the output, so the run job
# needs no repository checkout and always runs exactly the bundles that were built.
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
contexts=()

load_contexts() {
  local file="$1"
  [ -f "$file" ] || { echo "ERROR: migration context list not found: $file" >&2; exit 1; }
  mapfile -t contexts < <(grep -vE '^[[:space:]]*(#|$)' "$file" | tr -d '\r')
  [ "${#contexts[@]}" -gt 0 ] || { echo "ERROR: no migration contexts listed in $file" >&2; exit 1; }
}

bundle_name() { echo "efbundle-$(echo "$1" | tr '[:upper:]' '[:lower:]')"; }

build() {
  local provider="$1" output="$2"
  case "$provider" in SqlServer|PostgreSql) ;; *) echo "ERROR: unsupported provider '$provider'" >&2; exit 1 ;; esac
  load_contexts "${MIGRATION_CONTEXTS_FILE:-$repo_root/ops/migrations/contexts.txt}"
  mkdir -p "$output"
  for context in "${contexts[@]}"; do
    echo "Building ${context}${provider}DBContext bundle"
    dotnet ef migrations bundle \
      --project src/Backend/Persistence/LS.Persistence/LS.Persistence.csproj \
      --startup-project src/Backend/Api/LS.Api/LS.Api.csproj \
      --context "${context}${provider}DBContext" \
      --self-contained -r linux-x64 --configuration Release \
      -o "$output/$(bundle_name "$context")"
  done
  printf '%s\n' "${contexts[@]}" > "$output/migration-contexts.txt"
  cp "${BASH_SOURCE[0]}" "$output/migration-bundles.sh"
}

run_bundle() {
  local bundle="$1" connection="$2" max_attempts=6 delay_seconds=10 log_file
  for attempt in $(seq 1 "$max_attempts"); do
    echo "Running ${bundle} (attempt ${attempt}/${max_attempts})..."
    log_file="$(mktemp)"
    if "$bundle" --connection "$connection" 2>&1 | tee "$log_file"; then
      rm -f "$log_file"
      return 0
    fi
    if grep -q "A valid connection string was not found" "$log_file"; then
      echo "ERROR: ${bundle} could not resolve its design-time connection string; not retrying."
      rm -f "$log_file"
      return 1
    fi
    rm -f "$log_file"
    if [ "$attempt" -eq "$max_attempts" ]; then
      echo "ERROR: ${bundle} failed after ${max_attempts} attempts."
      return 1
    fi
    echo "Database may still be starting. Retrying in ${delay_seconds} seconds..."
    sleep "$delay_seconds"
    delay_seconds=$((delay_seconds * 2))
  done
}

run() {
  local dir="$1" connection="$2"
  load_contexts "$dir/migration-contexts.txt"
  for context in "${contexts[@]}"; do
    local bundle
    bundle="$dir/$(bundle_name "$context")"
    [ -f "$bundle" ] || { echo "ERROR: missing bundle $bundle" >&2; exit 1; }
    chmod +x "$bundle"
    run_bundle "$bundle" "$connection"
  done
}

case "${1:-}" in
  build) build "$2" "$3" ;;
  run) run "$2" "$3" ;;
  *) echo "Usage: $0 build <provider> <output-dir> | run <bundle-dir> <connection-string>" >&2; exit 2 ;;
esac
