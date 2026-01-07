@echo off
REM ============================================================================
REM VUTEQ Scanner - Log Cleanup Script
REM Author: Hassan
REM Date: 2026-01-07
REM Description: Cleans up old log files to free disk space
REM ============================================================================

echo ============================================================================
echo VUTEQ Scanner - Log Cleanup
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

set LOG_DIR=E:\VuteqDeploy\logs
set BACKEND_STDOUT=E:\VuteqDeploy\backend\logs
set DAYS_TO_KEEP=30

echo This script will delete log files older than %DAYS_TO_KEEP% days
echo.
echo Directories to clean:
echo   - %LOG_DIR%
echo   - %BACKEND_STDOUT%
echo.

set /p confirm="Continue? (Y/N): "
if /i not "%confirm%"=="Y" (
    echo Operation cancelled
    pause
    exit /b 0
)

echo.
echo [1/3] Checking current log sizes...
echo.

if exist "%LOG_DIR%" (
    echo Application logs:
    powershell -Command "'{0:N2} MB' -f ((Get-ChildItem -Path '%LOG_DIR%' -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB)"
)

if exist "%BACKEND_STDOUT%" (
    echo IIS stdout logs:
    powershell -Command "'{0:N2} MB' -f ((Get-ChildItem -Path '%BACKEND_STDOUT%' -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB)"
)

echo.
echo [2/3] Deleting old log files...
echo.

REM Delete old application logs
if exist "%LOG_DIR%" (
    forfiles /p "%LOG_DIR%" /s /m *.log /d -%DAYS_TO_KEEP% /c "cmd /c del @path" 2>nul
    if %errorLevel% equ 0 (
        echo Application logs cleaned
    ) else (
        echo No old application logs found
    )
)

REM Delete old IIS stdout logs
if exist "%BACKEND_STDOUT%" (
    forfiles /p "%BACKEND_STDOUT%" /s /m stdout_*.log /d -%DAYS_TO_KEEP% /c "cmd /c del @path" 2>nul
    if %errorLevel% equ 0 (
        echo IIS stdout logs cleaned
    ) else (
        echo No old IIS logs found
    )
)

echo.
echo [3/3] Archiving large current logs...
echo.

REM Archive logs larger than 50MB
for %%f in ("%LOG_DIR%\*.log") do (
    if %%~zf gtr 52428800 (
        echo Archiving: %%~nxf ^(%%~zf bytes^)
        move "%%f" "%%f.%date:~10,4%%date:~4,2%%date:~7,2%.bak"
    )
)

echo.
echo ============================================================================
echo Cleanup Complete
echo ============================================================================
echo.
echo Remaining log sizes:
echo.

if exist "%LOG_DIR%" (
    echo Application logs:
    powershell -Command "'{0:N2} MB' -f ((Get-ChildItem -Path '%LOG_DIR%' -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB)"
)

if exist "%BACKEND_STDOUT%" (
    echo IIS stdout logs:
    powershell -Command "'{0:N2} MB' -f ((Get-ChildItem -Path '%BACKEND_STDOUT%' -File -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB)"
)

echo.
pause
