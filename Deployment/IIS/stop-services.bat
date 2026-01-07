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

if "%NOPAUSE_MODE%"=="1" goto :SKIP_HEADER
echo ============================================================================
echo VUTEQ Scanner - Stopping Services
echo ============================================================================
echo.

:SKIP_HEADER

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 goto :NEED_ADMIN

set SITE_NAME=VUTEQ Scanner

REM ============================================================================
REM Step 1: Stop PM2 Frontend
REM ============================================================================
echo [1/3] Stopping PM2 Frontend...

REM Check if PM2 is installed
where pm2 >nul 2>&1
if %errorLevel% neq 0 goto :NO_PM2

REM PM2 is installed, stop frontend
if "%NOPAUSE_MODE%"=="1" goto :PM2_SILENT

REM Normal mode with output
pm2 stop vuteq-frontend 2>nul
echo Frontend stopped
pm2 save --force >nul 2>&1
goto :DO_IIS

:PM2_SILENT
REM Silent mode: redirect all output
pm2 stop vuteq-frontend >nul 2>&1
pm2 save --force >nul 2>&1
echo Frontend stopped
goto :DO_IIS

:NO_PM2
echo PM2 not found, skipping frontend stop
goto :DO_IIS

REM ============================================================================
REM Step 2: Stop IIS Site
REM ============================================================================
:DO_IIS
echo.
echo [2/3] Stopping IIS Site...
%systemroot%\system32\inetsrv\appcmd stop site "%SITE_NAME%" >nul 2>&1
echo IIS Site stopped

REM Check if we should show status
if "%NOPAUSE_MODE%"=="1" goto :SILENT_END

REM ============================================================================
REM Step 3: Show Status (Normal Mode)
REM ============================================================================
echo.
echo [3/3] Checking service status...
echo.

echo IIS Site Status:
%systemroot%\system32\inetsrv\appcmd list site "%SITE_NAME%"

echo.
echo PM2 Process Status:
where pm2 >nul 2>&1
if %errorLevel% neq 0 goto :NO_PM2_STATUS

pm2 list
goto :SHOW_HELP

:NO_PM2_STATUS
echo PM2 not available
goto :SHOW_HELP

:SHOW_HELP
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
goto :END

REM ============================================================================
REM Silent Mode End
REM ============================================================================
:SILENT_END
echo Services stopped successfully
goto :END

REM ============================================================================
REM Error Handlers
REM ============================================================================
:NEED_ADMIN
echo ERROR: This script must be run as Administrator!
echo Right-click and select "Run as administrator"
exit /b 1

REM ============================================================================
REM Normal Exit
REM ============================================================================
:END
endlocal
exit /b 0
