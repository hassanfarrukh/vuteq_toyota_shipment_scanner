@echo off
REM ==============================================================================
REM Stop Development Environment
REM ==============================================================================
REM Author: Hassan
REM Date: 2025-11-23
REM Description: Stops all Docker containers for development environment
REM ==============================================================================

echo.
echo ==============================================================================
echo Stopping VUTEQ Development Environment
echo ==============================================================================
echo.

cd /d D:\VUTEQ\FromHassan\Codes\Docker\development

echo Stopping all services...
docker-compose -f docker-compose.dev.yml down

if %errorlevel% equ 0 (
    echo.
    echo ==============================================================================
    echo All services stopped successfully!
    echo ==============================================================================
    echo.
) else (
    echo.
    echo ERROR: Failed to stop services
    echo.
    exit /b 1
)

pause
