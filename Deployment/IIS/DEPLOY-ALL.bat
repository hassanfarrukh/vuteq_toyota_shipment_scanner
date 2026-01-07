@echo off
REM ============================================================================
REM VUTEQ Scanner - Complete Deployment Orchestrator
REM Author: Hassan
REM Date: 2026-01-07
REM Description: Master script to deploy entire VUTEQ Scanner application
REM ============================================================================

echo.
echo ============================================================================
echo          VUTEQ SCANNER - COMPLETE DEPLOYMENT ORCHESTRATOR
echo ============================================================================
echo.
echo                          Author: Hassan
echo                        Date: 2026-01-07
echo.
echo ============================================================================
echo.

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo.
    echo  ERROR: Administrator privileges required!
    echo.
    echo  Please right-click this script and select "Run as administrator"
    echo.
    pause
    exit /b 1
)

echo This script will perform a complete deployment of VUTEQ Scanner.
echo.
echo Deployment Steps:
echo   1. Deploy Backend (ASP.NET Core)
echo   2. Deploy Frontend (Next.js)
echo   3. Configure IIS (if not already configured)
echo   4. Start all services
echo   5. Verify deployment
echo.
echo WARNING: This will stop existing services during deployment.
echo.

set /p confirm="Continue with deployment? (Y/N): "
if /i not "%confirm%"=="Y" (
    echo.
    echo Deployment cancelled by user.
    pause
    exit /b 0
)

echo.
echo ============================================================================
echo STEP 1/5: Stopping existing services
echo ============================================================================
echo.

call stop-services.bat nopause

echo.
echo Waiting 5 seconds for services to stop...
ping localhost -n 6 >nul

echo.
echo ============================================================================
echo STEP 2/5: Deploying Backend
echo ============================================================================
echo.

call deploy-backend.bat nopause

if %errorLevel% neq 0 (
    echo.
    echo ERROR: Backend deployment failed!
    echo Please check the error messages above.
    pause
    exit /b 1
)

echo.
echo ============================================================================
echo STEP 3/5: Deploying Frontend
echo ============================================================================
echo.

call deploy-frontend.bat nopause

if %errorLevel% neq 0 (
    echo.
    echo ERROR: Frontend deployment failed!
    echo Please check the error messages above.
    pause
    exit /b 1
)

echo.
echo ============================================================================
echo STEP 4/5: Configuring IIS
echo ============================================================================
echo.

echo Checking if IIS site exists...
%systemroot%\system32\inetsrv\appcmd list site "VUTEQ Scanner" >nul 2>&1

if %errorLevel% neq 0 (
    echo IIS site not found. Running configuration...
    powershell -ExecutionPolicy Bypass -File "%~dp0configure-iis.ps1"

    if %errorLevel% neq 0 (
        echo.
        echo ERROR: IIS configuration failed!
        echo Please check the error messages above.
        pause
        exit /b 1
    )
) else (
    echo IIS site already configured. Skipping IIS configuration.
    echo If you need to reconfigure, run configure-iis.ps1 manually.
)

echo.
echo ============================================================================
echo STEP 5/5: Starting Services
echo ============================================================================
echo.

call start-services.bat nopause

if %errorLevel% neq 0 (
    echo.
    echo WARNING: Service startup encountered issues.
    echo Please check the services manually.
)

echo.
echo ============================================================================
echo STEP 6/6: Verifying Deployment
echo ============================================================================
echo.

ping localhost -n 11 >nul

REM Verification tests
echo Running verification tests...
echo.

set TESTS_PASSED=0
set TESTS_FAILED=0

echo [Test 1/4] Frontend Service (http://localhost:3000)
powershell -Command "try { $r = Invoke-WebRequest -Uri 'http://localhost:3000' -TimeoutSec 10 -UseBasicParsing; if ($r.StatusCode -eq 200) { Write-Host '  PASSED' -ForegroundColor Green; exit 0 } else { Write-Host '  FAILED' -ForegroundColor Red; exit 1 } } catch { Write-Host '  FAILED:' $_.Exception.Message -ForegroundColor Red; exit 1 }"
if %errorLevel% equ 0 (set /a TESTS_PASSED+=1) else (set /a TESTS_FAILED+=1)

echo.
echo [Test 2/4] Backend Service (http://localhost:5000/api)
powershell -Command "try { $r = Invoke-WebRequest -Uri 'http://localhost:5000/api' -TimeoutSec 10 -UseBasicParsing; Write-Host '  PASSED' -ForegroundColor Green; exit 0 } catch { Write-Host '  FAILED:' $_.Exception.Message -ForegroundColor Red; exit 1 }"
if %errorLevel% equ 0 (set /a TESTS_PASSED+=1) else (set /a TESTS_FAILED+=1)

echo.
echo [Test 3/4] IIS Frontend Proxy (http://localhost)
powershell -Command "try { $r = Invoke-WebRequest -Uri 'http://localhost' -TimeoutSec 10 -UseBasicParsing; if ($r.StatusCode -eq 200) { Write-Host '  PASSED' -ForegroundColor Green; exit 0 } else { Write-Host '  FAILED' -ForegroundColor Red; exit 1 } } catch { Write-Host '  FAILED:' $_.Exception.Message -ForegroundColor Red; exit 1 }"
if %errorLevel% equ 0 (set /a TESTS_PASSED+=1) else (set /a TESTS_FAILED+=1)

echo.
echo [Test 4/4] IIS API Proxy (http://localhost/api)
powershell -Command "try { $r = Invoke-WebRequest -Uri 'http://localhost/api' -TimeoutSec 10 -UseBasicParsing; Write-Host '  PASSED' -ForegroundColor Green; exit 0 } catch { Write-Host '  FAILED:' $_.Exception.Message -ForegroundColor Red; exit 1 }"
if %errorLevel% equ 0 (set /a TESTS_PASSED+=1) else (set /a TESTS_FAILED+=1)

echo.
echo ============================================================================
echo                       DEPLOYMENT COMPLETE
echo ============================================================================
echo.
echo Verification Results: %TESTS_PASSED%/4 tests passed
echo.

if %TESTS_FAILED% gtr 0 (
    echo STATUS: DEPLOYED WITH WARNINGS
    echo.
    echo Some tests failed. Please check:
    echo   1. Run check-status.bat for detailed status
    echo   2. Run view-logs.bat to check for errors
    echo   3. Refer to README.md troubleshooting section
) else (
    echo STATUS: DEPLOYMENT SUCCESSFUL
    echo.
    echo All verification tests passed!
)

echo.
echo ============================================================================
echo Deployment Information
echo ============================================================================
echo.
echo Application URL: http://localhost
echo API Endpoint: http://localhost/api
echo.
echo Deployed Components:
echo   Backend:  C:\inetpub\vuteq\backend
echo   Frontend: C:\inetpub\vuteq\frontend
echo.
echo Log Locations:
echo   Backend:  C:\inetpub\vuteq\logs\backend.log
echo   Frontend: C:\inetpub\vuteq\logs\frontend-*.log
echo   IIS:      C:\inetpub\vuteq\backend\logs\stdout_*.log
echo.
echo Management Commands:
echo   Start services:   start-services.bat
echo   Stop services:    stop-services.bat
echo   Restart services: restart-services.bat
echo   Check status:     check-status.bat
echo   View logs:        view-logs.bat
echo.
echo PM2 Commands:
echo   View processes:   pm2 list
echo   View logs:        pm2 logs vuteq-frontend
echo   Restart:          pm2 restart vuteq-frontend
echo.
echo ============================================================================
echo.

set /p open="Open application in browser? (Y/N): "
if /i "%open%"=="Y" (
    start http://localhost
)

echo.
echo Deployment timestamp: %date% %time%
echo.
pause
