@echo off
REM ============================================================================
REM VUTEQ Scanner - Status Check Script
REM Author: Hassan
REM Date: 2026-01-07
REM Description: Checks status of all services and tests endpoints
REM ============================================================================

echo ============================================================================
echo VUTEQ Scanner - System Status Check
echo ============================================================================
echo.

set SITE_NAME=VUTEQ Scanner

echo [1/5] IIS Site Status
echo ----------------------------------------
%systemroot%\system32\inetsrv\appcmd list site "%SITE_NAME%"

echo.
echo [2/5] PM2 Process Status
echo ----------------------------------------
where pm2 >nul 2>&1
if %errorLevel% equ 0 (
    pm2 list
) else (
    echo ERROR: PM2 not found
)

echo.
echo [3/5] Port Status
echo ----------------------------------------
echo Checking if ports are in use...
echo.
echo Port 80 (IIS):
netstat -ano | findstr :80 | findstr LISTENING
echo.
echo Port 3000 (Frontend):
netstat -ano | findstr :3000 | findstr LISTENING
echo.
echo Port 5000 (Backend):
netstat -ano | findstr :5000 | findstr LISTENING

echo.
echo [4/5] Disk Space
echo ----------------------------------------
echo E:\VuteqDeploy\ folder size:
powershell -Command "'{0:N2} MB' -f ((Get-ChildItem -Path 'E:\VuteqDeploy' -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB)"

echo.
echo [5/5] Testing Endpoints
echo ----------------------------------------
echo.
echo Testing Frontend (http://localhost:3000)...
powershell -Command "try { $r = Invoke-WebRequest -Uri 'http://localhost:3000' -TimeoutSec 5 -UseBasicParsing; Write-Host 'Status:' $r.StatusCode '- OK' -ForegroundColor Green } catch { Write-Host 'Status: FAILED -' $_.Exception.Message -ForegroundColor Red }"

echo.
echo Testing Backend (http://localhost:5000/api)...
powershell -Command "try { $r = Invoke-WebRequest -Uri 'http://localhost:5000/api' -TimeoutSec 5 -UseBasicParsing; Write-Host 'Status:' $r.StatusCode '- OK' -ForegroundColor Green } catch { Write-Host 'Status: FAILED -' $_.Exception.Message -ForegroundColor Red }"

echo.
echo Testing IIS Proxy (http://localhost)...
powershell -Command "try { $r = Invoke-WebRequest -Uri 'http://localhost' -TimeoutSec 5 -UseBasicParsing; Write-Host 'Status:' $r.StatusCode '- OK' -ForegroundColor Green } catch { Write-Host 'Status: FAILED -' $_.Exception.Message -ForegroundColor Red }"

echo.
echo Testing IIS API Proxy (http://localhost/api)...
powershell -Command "try { $r = Invoke-WebRequest -Uri 'http://localhost/api' -TimeoutSec 5 -UseBasicParsing; Write-Host 'Status:' $r.StatusCode '- OK' -ForegroundColor Green } catch { Write-Host 'Status: FAILED -' $_.Exception.Message -ForegroundColor Red }"

echo.
echo ============================================================================
echo Status Check Complete
echo ============================================================================
echo.
echo Recent Logs:
echo   Frontend: E:\VuteqDeploy\logs\frontend\frontend-out.log
echo   Backend: E:\VuteqDeploy\logs\backend\backend.log
echo   IIS: E:\VuteqDeploy\backend\logs\stdout_*.log
echo.
pause
