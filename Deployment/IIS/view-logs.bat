@echo off
REM ============================================================================
REM VUTEQ Scanner - Log Viewer Script
REM Author: Hassan
REM Date: 2026-01-07
REM Description: Quick access to view application logs
REM ============================================================================

echo ============================================================================
echo VUTEQ Scanner - Log Viewer
echo ============================================================================
echo.
echo Select log to view:
echo.
echo [1] Frontend Output Log (PM2)
echo [2] Frontend Error Log (PM2)
echo [3] Backend Application Log
echo [4] IIS Stdout Logs
echo [5] PM2 Live Logs
echo [6] All Recent Errors
echo [7] Exit
echo.

set /p choice="Enter your choice (1-7): "

if "%choice%"=="1" goto frontend_out
if "%choice%"=="2" goto frontend_error
if "%choice%"=="3" goto backend
if "%choice%"=="4" goto iis
if "%choice%"=="5" goto pm2_live
if "%choice%"=="6" goto errors
if "%choice%"=="7" goto end
goto invalid

:frontend_out
echo.
echo ============================================================================
echo Frontend Output Log (Last 50 lines)
echo ============================================================================
echo.
if exist "C:\inetpub\vuteq\logs\frontend-out.log" (
    powershell -Command "Get-Content 'C:\inetpub\vuteq\logs\frontend-out.log' -Tail 50"
) else (
    echo Log file not found
)
pause
goto end

:frontend_error
echo.
echo ============================================================================
echo Frontend Error Log (Last 50 lines)
echo ============================================================================
echo.
if exist "C:\inetpub\vuteq\logs\frontend-error.log" (
    powershell -Command "Get-Content 'C:\inetpub\vuteq\logs\frontend-error.log' -Tail 50"
) else (
    echo Log file not found
)
pause
goto end

:backend
echo.
echo ============================================================================
echo Backend Application Log (Last 50 lines)
echo ============================================================================
echo.
if exist "C:\inetpub\vuteq\logs\backend.log" (
    powershell -Command "Get-Content 'C:\inetpub\vuteq\logs\backend.log' -Tail 50"
) else (
    echo Log file not found
)
pause
goto end

:iis
echo.
echo ============================================================================
echo IIS Stdout Logs (Most Recent)
echo ============================================================================
echo.
if exist "C:\inetpub\vuteq\backend\logs\" (
    dir /b /od "C:\inetpub\vuteq\backend\logs\stdout_*.log"
    echo.
    for /f %%f in ('dir /b /od "C:\inetpub\vuteq\backend\logs\stdout_*.log"') do set LATEST=%%f
    echo Showing: !LATEST!
    echo.
    type "C:\inetpub\vuteq\backend\logs\!LATEST!"
) else (
    echo Log directory not found
)
pause
goto end

:pm2_live
echo.
echo ============================================================================
echo PM2 Live Logs (Press Ctrl+C to exit)
echo ============================================================================
echo.
pm2 logs vuteq-frontend
goto end

:errors
echo.
echo ============================================================================
echo Recent Errors (All Logs)
echo ============================================================================
echo.
echo Searching for errors in all log files...
echo.
if exist "C:\inetpub\vuteq\logs\" (
    echo --- Frontend Errors ---
    powershell -Command "Get-Content 'C:\inetpub\vuteq\logs\frontend-*.log' -ErrorAction SilentlyContinue | Select-String -Pattern 'error|exception|failed' -CaseSensitive:$false | Select-Object -Last 20"
    echo.
    echo --- Backend Errors ---
    powershell -Command "Get-Content 'C:\inetpub\vuteq\logs\backend.log' -ErrorAction SilentlyContinue | Select-String -Pattern 'error|exception|failed' -CaseSensitive:$false | Select-Object -Last 20"
) else (
    echo Log directory not found
)
pause
goto end

:invalid
echo.
echo Invalid choice. Please run the script again.
pause
goto end

:end
