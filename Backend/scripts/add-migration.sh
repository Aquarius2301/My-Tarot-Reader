#!/usr/bin/env bash
# Create a new EF Core migration.
# Usage: ./add-migration.sh <MigrationName> [configuration]
#   MigrationName : required, e.g. InitialCreate, AddUserEntity
#   configuration : Debug (default) | Release
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/.."

if [ $# -lt 1 ]; then
  echo "Usage: $0 <MigrationName> [configuration]" >&2
  exit 1
fi

MIGRATION_NAME="$1"
CONFIG="${2:-Debug}"

echo "==> Restoring local tools"
dotnet tool restore

echo "==> Adding migration '$MIGRATION_NAME' ($CONFIG)"
dotnet ef migrations add "$MIGRATION_NAME" \
  --project src/Infrastructure \
  --startup-project src/Api \
  -c MyTarotReader.Infrastructure.Persistence.AppDbContext \
  -o Persistence/Migrations \
  --configuration "$CONFIG"

echo "==> Done. Migration files live in src/Infrastructure/Persistence/Migrations/"