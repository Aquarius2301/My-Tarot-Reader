@echo off
rem Apply EF Core migrations to the database (requires PostgreSQL running).
rem Usage: update-database.cmd [configuration]
rem   configuration: Debug (default) | Release
set CONFIG=%~1
if "%CONFIG%"=="" set CONFIG=Debug
cd /d "%~dp0.."

echo ==^> Restoring local tools
dotnet tool restore
if errorlevel 1 exit /b 1

echo ==^> Applying migrations (%CONFIG%)
dotnet ef database update ^
  --project src/Infrastructure ^
  --startup-project src/Api ^
  -c MyTarotReader.Infrastructure.Persistence.AppDbContext ^
  --configuration %CONFIG%
if errorlevel 1 exit /b 1

echo ==^> Done.