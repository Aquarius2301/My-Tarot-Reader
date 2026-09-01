#!/usr/bin/env bash
# Clean + rebuild the My Tarot Reader solution.
# Usage: ./clean-build.sh [configuration]
#   configuration: Debug (default) | Release
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/.."

CONFIG="${1:-Debug}"
SLN="MyTarotReader.sln"

echo "==> Cleaning ($CONFIG)"
dotnet clean "$SLN" -c "$CONFIG"

echo "==> Restoring"
dotnet restore "$SLN"

echo "==> Building ($CONFIG)"
dotnet build "$SLN" -c "$CONFIG" --no-restore
