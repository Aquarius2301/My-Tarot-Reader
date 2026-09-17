@echo off
rem Clean + rebuild the My Tarot Reader solution.
rem Usage: clean-build.cmd [configuration]
rem   configuration: Debug (default) | Release
set CONFIG=%~1
if "%CONFIG%"=="" set CONFIG=Debug
cd /d "%~dp0.."

set SLN=MyTarotReader.sln

echo ==^> Cleaning (%CONFIG%)
dotnet clean "%SLN%" -c %CONFIG%
if errorlevel 1 exit /b 1

echo ==^> Restoring
dotnet restore "%SLN%"
if errorlevel 1 exit /b 1

echo ==^> Building (%CONFIG%)
dotnet build "%SLN%" -c %CONFIG% --no-restore