@echo off
REM ==============================================================================
REM VUTEQ Development Environment - Start Script (Windows)
REM ==============================================================================
REM Author: Hassan
REM Date: 2025-11-11
REM Description: One-command startup script for VUTEQ development environment
REM ==============================================================================

setlocal enabledelayedexpansion

echo ======================================================================
echo VUTEQ Development Environment - Startup
echo ======================================================================
echo Author: Hassan
echo Date: %date% %time%
echo ======================================================================
echo.

REM Change to docker directory
cd /d "%~dp0.."

echo Working Directory: %CD%
echo.

REM Check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Docker is not running!
    echo Please start Docker Desktop and try again.
    pause
    exit /b 1
)

echo Docker Status: Running
echo.

REM Check if docker-compose.dev.yml exists
if not exist "docker-compose.dev.yml" (
    echo ERROR: docker-compose.dev.yml not found!
    echo Expected location: %CD%\docker-compose.dev.yml
    pause
    exit /b 1
)

echo Configuration: docker-compose.dev.yml found
echo.

REM Check if init files exist
if not exist "init-db.sql" (
    echo WARNING: init-db.sql not found!
    echo Database initialization may fail.
)

if not exist "entrypoint.sh" (
    echo WARNING: entrypoint.sh not found!
    echo Database initialization may fail.
)

echo.
echo ======================================================================
echo Starting Services...
echo ======================================================================
echo.

REM Start services
docker compose -f docker-compose.dev.yml up -d

echo.
echo ======================================================================
echo Waiting for services to be healthy...
echo ======================================================================
echo.

REM Wait for SQL Server
echo Checking SQL Server (5s timeout)...
set /a counter=0
:wait_sqlserver
timeout /t 1 /nobreak >nul
set /a counter+=1
docker inspect vuteq-sqlserver-dev 2>nul | find "healthy" >nul
if %errorlevel% equ 0 (
    echo SQL Server is healthy!
    goto check_api
)
if %counter% lss 5 (
    echo .
    goto wait_sqlserver
)
echo WARNING: SQL Server did not become healthy in 5s

:check_api
echo.
echo Checking API (5s timeout)...
set /a counter=0
:wait_api
timeout /t 1 /nobreak >nul
set /a counter+=1
docker inspect vuteq-api-dev 2>nul | find "healthy" >nul
if %errorlevel% equ 0 (
    echo API is healthy!
    goto check_frontend
)
if %counter% lss 5 (
    echo .
    goto wait_api
)
echo WARNING: API did not become healthy in 5s

:check_frontend
echo.
echo Checking Frontend (5s timeout)...
set /a counter=0
:wait_frontend
timeout /t 1 /nobreak >nul
set /a counter+=1
docker inspect vuteq-frontend-dev 2>nul | find "healthy" >nul
if %errorlevel% equ 0 (
    echo Frontend is healthy!
    goto show_status
)
if %counter% lss 5 (
    echo .
    goto wait_frontend
)
echo WARNING: Frontend did not become healthy in 5s

:show_status
echo.
echo ======================================================================
echo Service Status
echo ======================================================================
echo.

docker compose -f docker-compose.dev.yml ps

echo.
echo ======================================================================
echo Database Initialization Check
echo ======================================================================
echo.

REM Wait a bit more for database initialization
timeout /t 5 /nobreak >nul

REM Check if database was created
docker exec vuteq-sqlserver-dev /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -Q "SELECT name FROM sys.databases WHERE name = 'VUTEQ_Scanner'" -h -1 >nul 2>&1
if %errorlevel% equ 0 (
    echo Database: VUTEQ_Scanner - CREATED

    REM Get table count
    for /f %%i in ('docker exec vuteq-sqlserver-dev /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -d VUTEQ_Scanner -Q "SELECT COUNT(*) FROM sys.tables" -h -1 -W 2^>nul') do set TABLE_COUNT=%%i
    echo Tables: !TABLE_COUNT!

    REM Get user count
    for /f %%i in ('docker exec vuteq-sqlserver-dev /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -d VUTEQ_Scanner -Q "SELECT COUNT(*) FROM dbo.Users" -h -1 -W 2^>nul') do set USER_COUNT=%%i
    echo Sample Users: !USER_COUNT!
) else (
    echo WARNING: Database VUTEQ_Scanner not found!
    echo Check initialization logs: docker logs vuteq-sqlserver-dev
)

echo.
echo ======================================================================
echo VUTEQ Development Environment - READY
echo ======================================================================
echo.
echo Application URLs:
echo   Frontend:  http://localhost:3000
echo   Backend:   http://localhost:5000
echo   Swagger:   http://localhost:5000/swagger
echo   Health:    http://localhost:5000/health
echo.
echo Database Connection:
echo   Host:      localhost
echo   Port:      1433
echo   Database:  VUTEQ_Scanner
echo   User:      sa
echo   Password:  VuteqDev2025!
echo.
echo Sample Credentials:
echo   Admin:     admin / admin123
echo   Operator:  operator / operator123
echo.
echo Useful Commands:
echo   View logs:     docker logs -f vuteq-api-dev
echo   Stop all:      docker compose -f docker-compose.dev.yml down
echo   Reset data:    docker compose -f docker-compose.dev.yml down -v
echo   Restart:       docker compose -f docker-compose.dev.yml restart
echo.
echo Documentation:
echo   Dev Guide:     README_DEV.md
echo   DB Setup:      DATABASE_SETUP.md
echo.
echo ======================================================================
echo Happy Coding!
echo ======================================================================
echo.

pause
