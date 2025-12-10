@echo off
REM ==============================================================================
REM Database Rebuild Script
REM ==============================================================================
REM Author: Hassan
REM Date: 2025-11-23
REM Description: Rebuilds the VUTEQ Scanner database with fresh schema
REM ==============================================================================

setlocal enabledelayedexpansion

set "workDir=D:\VUTEQ\FromHassan\Codes\Docker\development"
set "containerName=vuteq-scanner-db"
set "password=Vuteq@Dev2025"

echo.
echo ==============================================================================
echo VUTEQ Scanner Database Rebuild Process
echo ==============================================================================
echo.

REM Change to working directory
echo [Step 1] Navigating to Docker directory...
cd /d "%workDir%"
echo Current directory: %CD%
echo.

REM Stop and remove containers with volumes
echo [Step 2] Stopping and removing containers with volumes...
docker-compose -f docker-compose.dev.yml down -v
if %errorlevel% neq 0 (
    echo ERROR: Failed to stop containers
    exit /b 1
)
echo Containers and volumes removed successfully
echo.

REM Start fresh containers
echo [Step 3] Starting containers with clean volumes...
docker-compose -f docker-compose.dev.yml up -d
if %errorlevel% neq 0 (
    echo ERROR: Failed to start containers
    exit /b 1
)
echo Containers started successfully
echo.

REM Wait for SQL Server initialization
echo [Step 4] Waiting for SQL Server initialization (40 seconds)...
timeout /t 40 /nobreak
echo.

REM Copy init script to container
echo [Step 5] Copying database initialization script...
docker cp database\init-db.sql %containerName%:/tmp/init-db.sql
if %errorlevel% neq 0 (
    echo ERROR: Failed to copy init script
    exit /b 1
)
echo Script copied successfully
echo.

REM Execute initialization script
echo [Step 6] Executing database initialization script...
docker exec %containerName% /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P %password% -C -i /tmp/init-db.sql
if %errorlevel% neq 0 (
    echo ERROR: Failed to execute init script
    exit /b 1
)
echo Database initialized successfully
echo.

REM Verify tables
echo [Step 7] Verifying database tables...
docker exec %containerName% /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P %password% -C -d VUTEQ_Scanner -Q "SELECT TOP 5 name FROM sys.tables ORDER BY name"
if %errorlevel% neq 0 (
    echo ERROR: Failed to query tables
    exit /b 1
)
echo.

REM Final summary
echo ==============================================================================
echo Database Rebuild Complete!
echo ==============================================================================
echo Connection Details:
echo   Host:     localhost
echo   Port:     1434
echo   Database: VUTEQ_Scanner
echo   User:     sa
echo   Password: %password%
echo ==============================================================================
echo.

endlocal
