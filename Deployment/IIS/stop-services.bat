@echo off
REM ============================================================================
REM VUTEQ Scanner - Stop Services Script
REM Author: Hassan
REM Date: 2026-01-07
REM Description: Stops all VUTEQ Scanner services (IIS + PM2)
REM ============================================================================

setlocal enabledelayedexpansion

REM Check if called with nopause parameter
set "NOPAUSE_MODE=0"
if /i "%1"=="nopause" set "NOPAUSE_MODE=1"

if "%NOPAUSE_MODE%"=="0" (
    echo ============================================================================
    echo VUTEQ Scanner - Stopping Services
    echo ============================================================================
    echo.
)

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script must be run as Administrator!
    echo Right-click and select "Run as administrator"
    exit /b 1
)

set SITE_NAME=VUTEQ Scanner

echo [1/3] Stopping PM2 Frontend...

REM Check if PM2 is installed
where pm2 >nul 2>&1
if !errorLevel! equ 0 (
    pm2 stop vuteq-frontend 2>nul
    if !errorLevel! equ 0 (
        echo Frontend stopped successfully
    ) else (
        echo Frontend was not running
    )

    REM Save PM2 process list
    pm2 save --force
) else (
    echo PM2 not found, skipping frontend stop
)

echo.
echo [2/3] Stopping IIS Site...
%systemroot%\system32\inetsrv\appcmd stop site "%SITE_NAME%"

if !errorLevel! equ 0 (
    echo IIS Site stopped successfully
) else (
    echo WARNING: Failed to stop IIS site or site was not running
)

if "%NOPAUSE_MODE%"=="0" (
    echo.
    echo [3/3] Checking service status...
    echo.

    echo IIS Site Status:
    %systemroot%\system32\inetsrv\appcmd list site "%SITE_NAME%"

    echo.
    echo PM2 Process Status:
    where pm2 >nul 2>&1
    if !errorLevel! equ 0 (
        pm2 list
    ) else (
        echo PM2 not available
    )

    echo.
    echo ============================================================================
    echo Services Stopped!
    echo ============================================================================
    echo.
    echo To start services again, run: start-services.bat
    echo.
    echo To completely remove PM2 process:
    echo   pm2 delete vuteq-frontend
    echo   pm2 save
    echo.
) else (
    echo Services stopped successfully
)

endlocal
exit /b 0
