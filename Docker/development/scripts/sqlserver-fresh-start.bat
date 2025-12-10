@echo off
REM ==============================================================================
REM VUTEQ SQL Server - Complete Fresh Start Script
REM ==============================================================================
REM Author: Hassan
REM Date: 2025-11-11
REM Description: Complete cleanup and fresh start for SQL Server container
REM              Resolves authentication failures due to old volume data
REM ==============================================================================

echo ========================================
echo VUTEQ SQL Server Fresh Start
echo ========================================
echo.

REM Step 1: Stop all containers
echo [STEP 1/9] Stopping containers...
cd /d F:\VUTEQ\FromHassan\Codes\Docker\development
docker compose -f docker-compose.dev.yml down
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to stop containers
    pause
    exit /b 1
)
echo SUCCESS: Containers stopped
echo.

REM Step 2: Force remove containers
echo [STEP 2/9] Force removing containers...
docker rm -f vuteq-sqlserver-dev vuteq-frontend-dev 2>nul
echo SUCCESS: Containers removed
echo.

REM Step 3: Remove volumes
echo [STEP 3/9] Removing volumes...
docker volume rm vuteq-sqlserver-dev-data 2>nul
docker volume prune -f
echo SUCCESS: Volumes removed
echo.

REM Step 4: Verify volumes are deleted
echo [STEP 4/9] Verifying volumes are deleted...
docker volume ls | findstr vuteq
if %ERRORLEVEL% EQU 0 (
    echo WARNING: Some VUTEQ volumes still exist!
    docker volume ls | findstr vuteq
    echo.
) else (
    echo SUCCESS: All VUTEQ volumes removed
)
echo.

REM Step 5: Start fresh containers
echo [STEP 5/9] Starting fresh containers...
cd /d F:\VUTEQ\FromHassan\Codes\Docker\development
docker compose -f docker-compose.dev.yml up -d
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to start containers
    pause
    exit /b 1
)
echo SUCCESS: Containers started
echo.

REM Step 6: Wait for SQL Server initialization
echo [STEP 6/9] Waiting 10 seconds for SQL Server to initialize...
echo SQL Server is creating system databases with new password...
timeout /t 10 /nobreak
echo.

REM Step 7: Test authentication
echo [STEP 7/9] Testing SQL Server authentication...
docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -C -Q "SELECT @@VERSION"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Authentication test failed!
    echo SQL Server may still be initializing. Wait 30 more seconds and try again.
    timeout /t 30 /nobreak
    docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -C -Q "SELECT @@VERSION"
    if %ERRORLEVEL% NEQ 0 (
        echo ERROR: Authentication still failing. Check Docker logs:
        echo docker logs vuteq-sqlserver-dev
        pause
        exit /b 1
    )
)
echo SUCCESS: Authentication working
echo.

REM Step 8: Copy and execute init script
echo [STEP 8/9] Copying and executing database initialization script...
docker cp init-db.sql vuteq-sqlserver-dev:/tmp/init-db.sql
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to copy init-db.sql
    pause
    exit /b 1
)

docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -C -i /tmp/init-db.sql
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to execute init-db.sql
    echo Check the init-db.sql file for syntax errors
    pause
    exit /b 1
)
echo SUCCESS: Database initialized
echo.

REM Step 9: Verify database and tables
echo [STEP 9/9] Verifying database and tables...
docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -C -d VUTEQ_Scanner -Q "SELECT TOP 5 name FROM sys.tables ORDER BY name"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to query database
    echo Database may not have been created properly
    pause
    exit /b 1
)
echo.

echo ========================================
echo FRESH START COMPLETED SUCCESSFULLY!
echo ========================================
echo.
echo SQL Server is ready with:
echo - Database: VUTEQ_Scanner
echo - Username: sa
echo - Password: VuteqDev2025!
echo - Port: 1433
echo.
echo Connection String:
echo Server=localhost,1433;Database=VUTEQ_Scanner;User Id=sa;Password=VuteqDev2025!;TrustServerCertificate=True;
echo.
echo You can now connect from:
echo - Azure Data Studio
echo - SQL Server Management Studio
echo - Application Backend
echo.
pause
