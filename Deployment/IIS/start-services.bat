@echo off
REM ============================================================================
REM VUTEQ Scanner - Start Services Script
REM Author: Hassan
REM Date: 2026-01-07
REM Description: Starts all VUTEQ Scanner services (IIS + PM2)
REM ============================================================================

setlocal

REM Check if called with nopause parameter
set "NOPAUSE_MODE=0"
if /i "%1"=="nopause" set "NOPAUSE_MODE=1"

if "%NOPAUSE_MODE%"=="1" goto :SKIP_HEADER
echo ============================================================================
echo VUTEQ Scanner - Starting Services
echo ============================================================================
echo.

:SKIP_HEADER

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 goto :NO_ADMIN

set FRONTEND_DEPLOY=E:\VuteqDeploy\frontend
set SITE_NAME=VUTEQ Scanner

echo [1/4] Starting IIS Site...
%systemroot%\system32\inetsrv\appcmd start site "%SITE_NAME%"
if %errorLevel% equ 0 goto :IIS_OK
echo WARNING: Failed to start IIS site
echo Please check IIS Manager for details
goto :PM2_START

:IIS_OK
echo IIS Site started successfully

:PM2_START
echo.
echo [2/4] Starting Frontend with PM2...

REM Check if PM2 is installed
where pm2 >nul 2>&1
if %errorLevel% neq 0 goto :NO_PM2

REM Delete existing process if running
pm2 delete vuteq-frontend >nul 2>&1

REM Check if ecosystem config exists
if not exist "%FRONTEND_DEPLOY%\ecosystem.config.js" goto :NO_ECOSYSTEM

REM Start frontend using ecosystem config
cd /d "%FRONTEND_DEPLOY%"
pm2 start ecosystem.config.js >nul 2>&1
if %errorLevel% neq 0 goto :PM2_FAILED

echo Frontend started successfully
pm2 save --force >nul 2>&1
goto :CHECK_STATUS

:PM2_FAILED
echo ERROR: Failed to start frontend
echo Check logs: E:\VuteqDeploy\logs\frontend\frontend-error.log
exit /b 1

:NO_ECOSYSTEM
echo ERROR: Frontend ecosystem.config.js not found!
echo Please run deploy-frontend.bat first
exit /b 1

:NO_PM2
echo ERROR: PM2 not found!
echo Please run install-prerequisites.bat first
exit /b 1

:NO_ADMIN
echo ERROR: This script must be run as Administrator!
echo Right-click and select "Run as administrator"
exit /b 1

:CHECK_STATUS
if "%NOPAUSE_MODE%"=="1" goto :QUIET_EXIT

echo.
echo [3/4] Checking service status...
echo.

REM Check IIS site status
echo IIS Site Status:
%systemroot%\system32\inetsrv\appcmd list site "%SITE_NAME%"

echo.
echo PM2 Process Status:
pm2 list

echo.
echo [4/4] Testing endpoints...
echo.

REM Wait for services to fully start
ping localhost -n 6 >nul

REM Test frontend
echo Testing Frontend (http://localhost:3000)...
powershell -Command "try { $r = Invoke-WebRequest -Uri 'http://localhost:3000' -UseBasicParsing; Write-Host 'Frontend: OK (Status:' $r.StatusCode ')' -ForegroundColor Green } catch { Write-Host 'Frontend: FAILED' -ForegroundColor Red }"

echo.
echo Testing Backend (http://localhost:5000/api)...
powershell -Command "try { $r = Invoke-WebRequest -Uri 'http://localhost:5000/api' -UseBasicParsing; Write-Host 'Backend: OK (Status:' $r.StatusCode ')' -ForegroundColor Green } catch { Write-Host 'Backend: FAILED' -ForegroundColor Red }"

echo.
echo Testing IIS Proxy (http://localhost)...
powershell -Command "try { $r = Invoke-WebRequest -Uri 'http://localhost' -UseBasicParsing; Write-Host 'IIS Proxy: OK (Status:' $r.StatusCode ')' -ForegroundColor Green } catch { Write-Host 'IIS Proxy: FAILED' -ForegroundColor Red }"

echo.
echo ============================================================================
echo Services Started!
echo ============================================================================
echo.
echo Access the application at: http://localhost
echo.
echo Service Management:
echo   - View PM2 logs: pm2 logs vuteq-frontend
echo   - View PM2 status: pm2 status
echo   - Restart frontend: pm2 restart vuteq-frontend
echo   - Stop all: run stop-services.bat
echo.
echo Log locations:
echo   - Frontend: E:\VuteqDeploy\logs\frontend\frontend-*.log
echo   - Backend: E:\VuteqDeploy\logs\backend\backend.log
echo   - IIS: E:\VuteqDeploy\backend\logs\stdout_*.log
echo.
endlocal
exit /b 0

:QUIET_EXIT
echo Services started successfully
endlocal
exit /b 0
