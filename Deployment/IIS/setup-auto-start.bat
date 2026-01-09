@echo off
REM ============================================================================
REM VUTEQ Scanner - Setup Auto-Start Task
REM Author: Hassan
REM Date: 2026-01-09
REM Description: Creates a Windows Scheduled Task to auto-start services on boot
REM ============================================================================

echo ============================================================================
echo VUTEQ Scanner - Setup Auto-Start Task
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

set TASK_NAME=VUTEQ Scanner Auto-Start
set SCRIPT_PATH=%~dp0start-services.bat

echo This will create a scheduled task that:
echo   - Runs at Windows startup
echo   - Runs with admin privileges
echo   - Runs whether user is logged on or not
echo.
echo Task Name: %TASK_NAME%
echo Script: %SCRIPT_PATH%
echo.

REM Delete existing task if exists
schtasks /query /tn "%TASK_NAME%" >nul 2>&1
if %errorLevel% equ 0 (
    echo Removing existing task...
    schtasks /delete /tn "%TASK_NAME%" /f >nul 2>&1
)

echo Creating scheduled task...
echo.
echo NOTE: You will be prompted for your Windows password.
echo       This is required to run the task when not logged in.
echo.

schtasks /create ^
    /tn "%TASK_NAME%" ^
    /tr "\"%SCRIPT_PATH%\" nopause" ^
    /sc onstart ^
    /ru "%USERNAME%" ^
    /rl highest ^
    /f

if %errorLevel% neq 0 (
    echo.
    echo ERROR: Failed to create scheduled task!
    echo.
    echo Try creating it manually:
    echo   1. Open Task Scheduler
    echo   2. Create Basic Task
    echo   3. Trigger: When the computer starts
    echo   4. Action: Start a program
    echo   5. Program: %SCRIPT_PATH%
    echo   6. Arguments: nopause
    echo   7. Check "Run with highest privileges"
    echo   8. Change to "Run whether user is logged on or not"
    pause
    exit /b 1
)

echo.
echo ============================================================================
echo Task Created Successfully!
echo ============================================================================
echo.
echo Task Name: %TASK_NAME%
echo Trigger: At system startup
echo Action: %SCRIPT_PATH%
echo.
echo IMPORTANT: To enable "Run whether user is logged on or not":
echo   1. Open Task Scheduler (taskschd.msc)
echo   2. Find "%TASK_NAME%" under Task Scheduler Library
echo   3. Right-click and select "Properties"
echo   4. Under "Security options", select "Run whether user is logged on or not"
echo   5. Click OK and enter your password
echo.
echo To test the task manually:
echo   schtasks /run /tn "%TASK_NAME%"
echo.
echo To view task status:
echo   schtasks /query /tn "%TASK_NAME%" /v
echo.
echo To remove the task:
echo   schtasks /delete /tn "%TASK_NAME%" /f
echo.

pause
exit /b 0
