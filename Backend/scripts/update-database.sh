#!/usr/bin/env bash
# Apply EF Core migrations to the database (requires SQL Server running: 'docker compose up -d').
# Usage: ./update-database.sh [configuration]
#   configuration : Debug (default) | Release
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/.."

CONFIG="${1:-Debug}"

echo "==> Restoring local tools"
dotnet tool restore

echo "==> Applying migrations ($CONFIG)"
dotnet ef database update \
  --project src/Infrastructure \
  --startup-project src/Api \
  -c MyTarotReader.Infrastructure.Persistence.AppDbContext \
  --configuration "$CONFIG"

echo "==> Done."