@echo off
REM ==============================================================================
REM VUTEQ SQL Server - Verification Script
REM ==============================================================================
REM Author: Hassan
REM Date: 2025-11-11
REM Description: Verify SQL Server is running correctly with proper database
REM ==============================================================================

echo ========================================
echo VUTEQ SQL Server Verification
echo ========================================
echo.

REM Check if container is running
echo [CHECK 1] Verifying container is running...
docker ps --filter "name=vuteq-sqlserver-dev" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Docker is not running or container does not exist
    pause
    exit /b 1
)
echo.

REM Check SQL Server version
echo [CHECK 2] Checking SQL Server version...
docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -C -Q "SELECT @@VERSION AS SqlServerVersion"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Cannot connect to SQL Server
    echo Check password or wait for SQL Server to finish initializing
    pause
    exit /b 1
)
echo.

REM Check if database exists
echo [CHECK 3] Checking if VUTEQ_Scanner database exists...
docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -C -Q "SELECT name FROM sys.databases WHERE name = 'VUTEQ_Scanner'"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Cannot query databases
    pause
    exit /b 1
)
echo.

REM List all tables
echo [CHECK 4] Listing all tables in VUTEQ_Scanner database...
docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -C -d VUTEQ_Scanner -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Cannot query tables
    pause
    exit /b 1
)
echo.

REM Check for tbl prefix tables
echo [CHECK 5] Verifying tables have 'tbl' prefix...
docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -C -d VUTEQ_Scanner -Q "SELECT COUNT(*) AS TablesWithTblPrefix FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'tbl%'"
echo.

REM Check sample data
echo [CHECK 6] Checking for default admin user...
docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -C -d VUTEQ_Scanner -Q "SELECT UserId, Username, Name, Role FROM tblUserMaster WHERE Username = 'admin'"
echo.

REM Check volumes
echo [CHECK 7] Listing Docker volumes...
docker volume ls | findstr vuteq
echo.

REM Check logs for errors
echo [CHECK 8] Checking recent SQL Server logs for errors...
docker logs --tail 20 vuteq-sqlserver-dev
echo.

echo ========================================
echo VERIFICATION COMPLETE
echo ========================================
echo.
echo Connection Details:
echo - Host: localhost
echo - Port: 1433
echo - Database: VUTEQ_Scanner
echo - Username: sa
echo - Password: VuteqDev2025!
echo.
echo Connection String:
echo Server=localhost,1433;Database=VUTEQ_Scanner;User Id=sa;Password=VuteqDev2025!;TrustServerCertificate=True;
echo.
pause
