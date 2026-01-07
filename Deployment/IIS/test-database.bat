@echo off
REM ============================================================================
REM VUTEQ Scanner - Database Connection Test Script
REM Author: Hassan
REM Date: 2026-01-07
REM Description: Tests SQL Server connectivity and database status
REM ============================================================================

echo ============================================================================
echo VUTEQ Scanner - Database Connection Test
echo ============================================================================
echo.

set DB_SERVER=localhost
set DB_NAME=VUTEQ_Scanner
set DB_USER=VuteqApp
set DB_PASS=Vuteq@Prod@2026@CoffeCup!!

echo Testing connection to:
echo   Server: %DB_SERVER%
echo   Database: %DB_NAME%
echo   User: %DB_USER%
echo.

echo [1/5] Testing SQL Server availability...
echo ----------------------------------------
sqlcmd -S %DB_SERVER% -Q "SELECT @@VERSION" >nul 2>&1
if %errorLevel% equ 0 (
    echo SQL Server is running
) else (
    echo ERROR: SQL Server is not accessible
    echo Please check if SQL Server service is running
    pause
    exit /b 1
)

echo.
echo [2/5] Testing database login...
echo ----------------------------------------
sqlcmd -S %DB_SERVER% -U %DB_USER% -P "%DB_PASS%" -Q "SELECT 'Login successful' AS Status" -h -1 2>nul
if %errorLevel% equ 0 (
    echo Login successful
) else (
    echo ERROR: Login failed
    echo Please verify username and password
    pause
    exit /b 1
)

echo.
echo [3/5] Verifying database exists...
echo ----------------------------------------
sqlcmd -S %DB_SERVER% -U %DB_USER% -P "%DB_PASS%" -Q "SELECT name FROM sys.databases WHERE name='%DB_NAME%'" -h -1 2>nul | findstr /C:"%DB_NAME%" >nul
if %errorLevel% equ 0 (
    echo Database '%DB_NAME%' exists
) else (
    echo ERROR: Database '%DB_NAME%' not found
    echo Please create the database first
    pause
    exit /b 1
)

echo.
echo [4/5] Checking database tables...
echo ----------------------------------------
sqlcmd -S %DB_SERVER% -U %DB_USER% -P "%DB_PASS%" -d %DB_NAME% -Q "SELECT COUNT(*) AS TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'" -h -1
if %errorLevel% equ 0 (
    echo Table count retrieved successfully
) else (
    echo WARNING: Could not retrieve table count
)

echo.
echo [5/5] Testing connection string format...
echo ----------------------------------------
echo.
echo Connection String (for appsettings.json):
echo Server=%DB_SERVER%;Database=%DB_NAME%;User Id=%DB_USER%;Password=%DB_PASS%;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False;
echo.

echo.
echo ============================================================================
echo Database Connection Test Complete
echo ============================================================================
echo.
echo Additional Database Information:
echo.

sqlcmd -S %DB_SERVER% -U %DB_USER% -P "%DB_PASS%" -d %DB_NAME% -Q "SELECT name AS TableName FROM sys.tables ORDER BY name" -W
echo.

echo Database Size:
sqlcmd -S %DB_SERVER% -U %DB_USER% -P "%DB_PASS%" -d %DB_NAME% -Q "EXEC sp_spaceused" -W
echo.

pause
