@echo off
REM ==============================================================================
REM View Development Logs
REM ==============================================================================
REM Author: Hassan
REM Date: 2025-11-23
REM Description: View real-time logs from all Docker containers
REM ==============================================================================

echo.
echo ==============================================================================
echo VUTEQ Development Environment - Live Logs
echo ==============================================================================
echo Press Ctrl+C to exit
echo.

cd /d D:\VUTEQ\FromHassan\Codes\Docker\development

docker-compose -f docker-compose.dev.yml logs -f
