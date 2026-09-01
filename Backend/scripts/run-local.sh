#!/usr/bin/env bash
# Run the My Tarot Reader solution locally.
# Usage: ./run-local.sh [configuration]
#   configuration: Debug (default) | Release
set -euo pipefail   

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/.."

echo "==> Running API (dotnet run)"
dotnet run --project src/Api 