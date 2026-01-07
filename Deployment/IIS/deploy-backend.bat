@echo off
REM ============================================================================
REM VUTEQ Scanner - Backend Deployment Script
REM Author: Hassan
REM Date: 2026-01-07
REM Description: Builds and deploys ASP.NET Core backend to IIS
REM ============================================================================

echo ============================================================================
echo VUTEQ Scanner - Deploying Backend
echo ============================================================================
echo.

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script must be run as Administrator!
    echo Right-click and select "Run as administrator"
    pause
    exit /b 1
)

REM Configuration
set BACKEND_SOURCE=D:\VUTEQ\FromHassan\Codes\Backend
set DEPLOY_ROOT=C:\inetpub\vuteq
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
    pause
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
dotnet publish -c Release -o publish --no-self-contained

if %errorLevel% neq 0 (
    echo ERROR: Build failed!
    pause
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
echo       "Path": "C:\\inetpub\\vuteq\\logs\\backend.log",
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
if not exist "C:\inetpub\vuteq\logs" mkdir "C:\inetpub\vuteq\logs"

echo.
echo [6/6] Setting folder permissions...

REM Grant IIS_IUSRS full control
icacls "%BACKEND_DEPLOY%" /grant "IIS_IUSRS:(OI)(CI)F" /T /Q
icacls "C:\inetpub\vuteq\logs" /grant "IIS_IUSRS:(OI)(CI)F" /T /Q

REM Grant NETWORK SERVICE permissions
icacls "%BACKEND_DEPLOY%" /grant "NETWORK SERVICE:(OI)(CI)F" /T /Q
icacls "C:\inetpub\vuteq\logs" /grant "NETWORK SERVICE:(OI)(CI)F" /T /Q

echo Permissions set successfully

echo.
echo ============================================================================
echo Backend Deployment Complete!
echo ============================================================================
echo.
echo Deployed to: %BACKEND_DEPLOY%
echo Configuration: %BACKEND_DEPLOY%\appsettings.Production.json
echo Logs: C:\inetpub\vuteq\logs\backend.log
echo.
echo Next steps:
echo   1. Run deploy-frontend.bat
echo   2. Run configure-iis.bat (if not done already)
echo   3. Run start-services.bat
echo.
pause
