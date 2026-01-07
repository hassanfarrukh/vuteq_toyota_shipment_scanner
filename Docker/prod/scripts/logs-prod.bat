@echo off
REM Author: Hassan
REM Date: 2026-01-07
REM View production Docker logs

echo ================================================
echo  VUTEQ Scanner Production Logs
echo ================================================
echo.
echo Press Ctrl+C to exit logs
echo.

cd /d "%~dp0.."

docker-compose -f docker-compose.prod.yml logs -f
