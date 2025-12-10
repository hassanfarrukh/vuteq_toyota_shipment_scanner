@echo off
REM ==============================================================================
REM VUTEQ SQL Server - Troubleshooting Script
REM ==============================================================================
REM Author: Hassan
REM Date: 2025-11-11
REM Description: Diagnose SQL Server connection and authentication issues
REM ==============================================================================

echo ========================================
echo VUTEQ SQL Server Troubleshooting
echo ========================================
echo.

:MENU
echo Select troubleshooting action:
echo.
echo 1. Check container status and logs
echo 2. Test SQL Server connection
echo 3. View SQL Server error logs
echo 4. Check volume mounts
echo 5. Check environment variables
echo 6. Test with different password (old data check)
echo 7. View container resource usage
echo 8. Full diagnostic report
echo 9. Exit
echo.
set /p choice="Enter choice (1-9): "

if "%choice%"=="1" goto CHECK_STATUS
if "%choice%"=="2" goto TEST_CONNECTION
if "%choice%"=="3" goto VIEW_LOGS
if "%choice%"=="4" goto CHECK_VOLUMES
if "%choice%"=="5" goto CHECK_ENV
if "%choice%"=="6" goto TEST_OLD_PASSWORD
if "%choice%"=="7" goto RESOURCE_USAGE
if "%choice%"=="8" goto FULL_DIAGNOSTIC
if "%choice%"=="9" goto END
goto MENU

:CHECK_STATUS
echo.
echo === Container Status ===
docker ps -a --filter "name=vuteq-sqlserver-dev" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}\t{{.Size}}"
echo.
echo === Last 30 log lines ===
docker logs --tail 30 vuteq-sqlserver-dev
echo.
pause
goto MENU

:TEST_CONNECTION
echo.
echo === Testing SQL Server Connection ===
echo Testing with password: VuteqDev2025!
docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -C -Q "SELECT @@VERSION, GETDATE() AS CurrentTime"
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo CONNECTION FAILED!
    echo Possible causes:
    echo - SQL Server still initializing (wait 60 seconds)
    echo - Wrong password (old volume data)
    echo - SQL Server crashed (check logs)
)
echo.
pause
goto MENU

:VIEW_LOGS
echo.
echo === SQL Server Error Logs ===
docker exec vuteq-sqlserver-dev cat /var/opt/mssql/log/errorlog
echo.
pause
goto MENU

:CHECK_VOLUMES
echo.
echo === Docker Volumes ===
docker volume ls
echo.
echo === Volume Inspection ===
docker volume inspect vuteq-sqlserver-dev-data 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Volume does not exist - this is GOOD for fresh start
) else (
    echo Volume exists - contains SQL Server data
)
echo.
pause
goto MENU

:CHECK_ENV
echo.
echo === Container Environment Variables ===
docker exec vuteq-sqlserver-dev env | findstr /I "MSSQL SA_PASSWORD ACCEPT"
echo.
echo === Expected Environment ===
echo ACCEPT_EULA=Y
echo SA_PASSWORD=VuteqDev2025!
echo MSSQL_PID=Developer
echo MSSQL_TCP_PORT=1433
echo.
pause
goto MENU

:TEST_OLD_PASSWORD
echo.
echo === Testing with OLD password (if volume has old data) ===
echo This test helps determine if old volume data is causing issues
echo.
set /p oldpwd="Enter old password to test (or press Enter to skip): "
if "%oldpwd%"=="" goto MENU
docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P %oldpwd% -C -Q "SELECT 'OLD PASSWORD WORKS' AS Result"
if %ERRORLEVEL% EQU 0 (
    echo.
    echo OLD PASSWORD WORKS!
    echo This means the volume contains OLD data with OLD password
    echo You MUST delete the volume and restart fresh
    echo Run: sqlserver-fresh-start.bat
) else (
    echo.
    echo Old password does not work
    echo Volume might be clean or SQL Server is not ready
)
echo.
pause
goto MENU

:RESOURCE_USAGE
echo.
echo === Container Resource Usage ===
docker stats --no-stream vuteq-sqlserver-dev
echo.
pause
goto MENU

:FULL_DIAGNOSTIC
echo.
echo ========================================
echo FULL DIAGNOSTIC REPORT
echo ========================================
echo.

echo === Docker Version ===
docker --version
echo.

echo === Container Status ===
docker ps -a --filter "name=vuteq-sqlserver-dev"
echo.

echo === Container Logs (Last 50 lines) ===
docker logs --tail 50 vuteq-sqlserver-dev
echo.

echo === SQL Server Connection Test ===
docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -C -Q "SELECT @@VERSION"
echo.

echo === Database List ===
docker exec vuteq-sqlserver-dev /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P VuteqDev2025! -C -Q "SELECT name FROM sys.databases"
echo.

echo === Volume Information ===
docker volume ls | findstr vuteq
echo.

echo === Network Ports ===
netstat -an | findstr :1433
echo.

echo.
echo === Diagnostic Complete ===
echo.
pause
goto MENU

:END
echo.
echo Exiting troubleshooting tool...
exit /b 0
