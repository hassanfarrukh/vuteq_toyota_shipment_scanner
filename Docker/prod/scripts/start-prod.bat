@echo off
REM Author: Hassan
REM Date: 2026-01-07
REM Start production Docker environment

echo ================================================
echo  Starting VUTEQ Scanner Production Environment
echo ================================================
echo.

cd /d "%~dp0.."

echo [1/3] Building production images...
docker-compose -f docker-compose.prod.yml build

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo.
echo [2/3] Starting containers...
docker-compose -f docker-compose.prod.yml up -d

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Failed to start containers!
    pause
    exit /b 1
)

echo.
echo [3/3] Waiting for services to be healthy...
timeout /t 10 /nobreak >nul

echo.
echo ================================================
echo  Production Environment Started Successfully
echo ================================================
echo.
echo Services:
echo   - Nginx Reverse Proxy: http://localhost
echo   - API Backend: http://localhost/api
echo   - Health Check: http://localhost/health
echo.
echo To view logs: logs-prod.bat
echo To stop: stop-prod.bat
echo.
pause
