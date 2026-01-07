@echo off
REM Author: Hassan
REM Date: 2026-01-07
REM Stop production Docker environment

echo ================================================
echo  Stopping VUTEQ Scanner Production Environment
echo ================================================
echo.

cd /d "%~dp0.."

echo Stopping all containers...
docker-compose -f docker-compose.prod.yml down

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Failed to stop containers!
    pause
    exit /b 1
)

echo.
echo ================================================
echo  Production Environment Stopped Successfully
echo ================================================
echo.
pause
