@echo off
REM ============================================================================
REM VUTEQ Scanner - Restart Services Script
REM Author: Hassan
REM Date: 2026-01-07
REM Description: Gracefully restarts all VUTEQ Scanner services
REM ============================================================================

echo ============================================================================
echo VUTEQ Scanner - Restarting Services
echo ============================================================================
echo.

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script must be run as Administrator!
    echo Right-click and select "Run as administrator"
    pause
    exit /b 1
)

echo [1/2] Stopping services...
call stop-services.bat

echo.
echo Waiting 5 seconds before restart...
timeout /t 5 /nobreak >nul

echo.
echo [2/2] Starting services...
call start-services.bat

echo.
echo ============================================================================
echo Services Restarted!
echo ============================================================================
echo.
pause
