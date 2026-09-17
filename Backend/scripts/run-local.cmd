@echo off
rem Run the My Tarot Reader solution locally.
rem Usage: run-local.cmd [configuration]
rem   configuration: Debug (default) | Release
cd /d "%~dp0.."

echo ==^> Running API (dotnet run)
dotnet run --project src/Api