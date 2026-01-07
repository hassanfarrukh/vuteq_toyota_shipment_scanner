@echo off
REM ============================================================================
REM VUTEQ Scanner - Backend Deployment Script
REM Author: Hassan
REM Date: 2026-01-07
REM Description: Builds and deploys ASP.NET Core backend to IIS
REM ============================================================================

setlocal enabledelayedexpansion

REM Check if called with nopause parameter
set "NOPAUSE_MODE=0"
if /i "%1"=="nopause" set "NOPAUSE_MODE=1"

if "%NOPAUSE_MODE%"=="0" (
    echo ============================================================================
    echo VUTEQ Scanner - Deploying Backend
    echo ============================================================================
    echo.
)

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script must be run as Administrator!
    echo Right-click and select "Run as administrator"
    exit /b 1
)

REM Configuration - Use relative paths from script location
set SCRIPT_DIR=%~dp0
set BACKEND_SOURCE=%SCRIPT_DIR%..\..\Backend
set DEPLOY_ROOT=E:\VuteqDeploy
set BACKEND_DEPLOY=%DEPLOY_ROOT%\backend
set BACKUP_ROOT=%DEPLOY_ROOT%\backups
set TIMESTAMP=%date:~10,4%-%date:~4,2%-%date:~7,2%_%time:~0,2%-%time:~3,2%-%time:~6,2%
set TIMESTAMP=%TIMESTAMP: =0%

echo Source: %BACKEND_SOURCE%
echo Deploy: %BACKEND_DEPLOY%
echo.

REM Verify source directory exists
if not exist "%BACKEND_SOURCE%" (
    echo ERROR: Backend source directory not found: %BACKEND_SOURCE%
    exit /b 1
)

REM Create deployment directories
echo [1/6] Creating deployment directories...
if not exist "%DEPLOY_ROOT%" mkdir "%DEPLOY_ROOT%"
if not exist "%BACKUP_ROOT%" mkdir "%BACKUP_ROOT%"

REM Backup existing deployment
if exist "%BACKEND_DEPLOY%" (
    echo [2/6] Backing up existing deployment...
    set BACKUP_DIR=%BACKUP_ROOT%\backend_%TIMESTAMP%
    mkdir "!BACKUP_DIR!"
    xcopy "%BACKEND_DEPLOY%" "!BACKUP_DIR!" /E /I /Q
    echo Backup created: !BACKUP_DIR!
) else (
    echo [2/6] No existing deployment to backup
)

echo.
echo [3/6] Building backend in Release mode...
cd /d "%BACKEND_SOURCE%"

REM Clean previous builds
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "publish" rmdir /s /q "publish"

REM Build and publish
call dotnet publish -c Release -o publish --no-self-contained

if %errorLevel% neq 0 (
    echo ERROR: Build failed!
    exit /b 1
)

echo.
echo [4/6] Deploying to %BACKEND_DEPLOY%...

REM Stop IIS site if running
echo Stopping IIS site...
%systemroot%\system32\inetsrv\appcmd stop site "VUTEQ Scanner" >nul 2>&1

REM Remove old deployment
if exist "%BACKEND_DEPLOY%" (
    rmdir /s /q "%BACKEND_DEPLOY%"
)

REM Create deployment directory
mkdir "%BACKEND_DEPLOY%"

REM Copy published files
xcopy "%BACKEND_SOURCE%\publish" "%BACKEND_DEPLOY%" /E /I /Q

echo.
echo [5/6] Creating production configuration...

REM Create appsettings.Production.json
(
echo {
echo   "Logging": {
echo     "LogLevel": {
echo       "Default": "Information",
echo       "Microsoft.AspNetCore": "Warning",
echo       "Microsoft.EntityFrameworkCore": "Warning"
echo     },
echo     "File": {
echo       "Path": "E:\\VuteqDeploy\\logs\\backend\\backend.log",
echo       "Append": true,
echo       "MinLevel": "Information",
echo       "FileSizeLimitBytes": 10485760,
echo       "MaxRollingFiles": 10
echo     }
echo   },
echo   "AllowedHosts": "*",
echo   "ConnectionStrings": {
echo     "DefaultConnection": "Server=localhost;Database=VUTEQ_Scanner;User Id=VuteqApp;Password=Vuteq@Prod@2026@CoffeCup!!;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False;"
echo   },
echo   "JwtSettings": {
echo     "SecretKey": "VuteqProdSecretKey2026!MinLength32CharsSecure!",
echo     "Issuer": "VUTEQScanner",
echo     "Audience": "VUTEQScannerUsers",
echo     "ExpiryMinutes": 480
echo   },
echo   "Cors": {
echo     "AllowedOrigins": [
echo       "http://localhost",
echo       "http://127.0.0.1"
echo     ]
echo   }
echo }
) > "%BACKEND_DEPLOY%\appsettings.Production.json"

echo Production configuration created

REM Create logs directory
if not exist "E:\VuteqDeploy\logs\backend" mkdir "E:\VuteqDeploy\logs\backend"

REM Copy clean web.config template (prevents n++ corruption from auto-generated config)
echo.
echo Copying clean web.config template...
if exist "%SCRIPT_DIR%web.config.backend.template" (
    copy /Y "%SCRIPT_DIR%web.config.backend.template" "%BACKEND_DEPLOY%\web.config" >nul
    echo Clean web.config deployed (prevents n++ corruption)
) else (
    echo WARNING: web.config.backend.template not found - using auto-generated web.config
)

echo.
echo [6/6] Setting folder permissions...

REM Grant IIS_IUSRS full control
icacls "%BACKEND_DEPLOY%" /grant "IIS_IUSRS:(OI)(CI)F" /T /Q
icacls "E:\VuteqDeploy\logs" /grant "IIS_IUSRS:(OI)(CI)F" /T /Q

REM Grant NETWORK SERVICE permissions
icacls "%BACKEND_DEPLOY%" /grant "NETWORK SERVICE:(OI)(CI)F" /T /Q
icacls "E:\VuteqDeploy\logs" /grant "NETWORK SERVICE:(OI)(CI)F" /T /Q

echo Permissions set successfully

if "%NOPAUSE_MODE%"=="0" (
    echo.
    echo ============================================================================
    echo Backend Deployment Complete!
    echo ============================================================================
    echo.
    echo Deployed to: %BACKEND_DEPLOY%
    echo Configuration: %BACKEND_DEPLOY%\appsettings.Production.json
    echo Logs: E:\VuteqDeploy\logs\backend\backend.log
    echo.
    echo Next steps:
    echo   1. Run deploy-frontend.bat
    echo   2. Run configure-iis.bat (if not done already)
    echo   3. Run start-services.bat
    echo.
) else (
    echo Backend deployed successfully
)

REM Return to original directory
cd /d "%SCRIPT_DIR%"

endlocal
exit /b 0
