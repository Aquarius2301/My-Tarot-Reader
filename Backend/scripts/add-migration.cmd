@echo off
rem Create a new EF Core migration.
rem Usage: add-migration.cmd <MigrationName> [configuration]
rem   MigrationName : required, e.g. InitialCreate, AddUserEntity
rem   configuration : Debug (default) | Release
if "%~1"=="" (
  echo Usage: %~nx0 ^<MigrationName^> [configuration]
  exit /b 1
)
set MIGRATION_NAME=%~1
set CONFIG=%~2
if "%CONFIG%"=="" set CONFIG=Debug
cd /d "%~dp0.."

echo ==^> Restoring local tools
dotnet tool restore
if errorlevel 1 exit /b 1

echo ==^> Adding migration '%MIGRATION_NAME%' (%CONFIG%)
dotnet ef migrations add "%MIGRATION_NAME%" ^
  --project src/Infrastructure ^
  --startup-project src/Api ^
  -c MyTarotReader.Infrastructure.Persistence.AppDbContext ^
  -o Persistence/Migrations ^
  --configuration %CONFIG%
if errorlevel 1 exit /b 1

echo ==^> Done. Migration files live in src/Infrastructure/Persistence/Migrations/