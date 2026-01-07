@echo off
REM ============================================================================
REM VUTEQ Scanner - Frontend Deployment Script
REM Author: Hassan
REM Date: 2026-01-07
REM Description: Builds and deploys Next.js frontend with PM2
REM ============================================================================

setlocal enabledelayedexpansion

REM Check if called with nopause parameter
set "NOPAUSE_MODE=0"
if /i "%1"=="nopause" set "NOPAUSE_MODE=1"

if "%NOPAUSE_MODE%"=="0" (
    echo ============================================================================
    echo VUTEQ Scanner - Deploying Frontend
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
set FRONTEND_SOURCE=%SCRIPT_DIR%..\..\ScannerApp
set DEPLOY_ROOT=E:\VuteqDeploy
set FRONTEND_DEPLOY=%DEPLOY_ROOT%\frontend
set BACKUP_ROOT=%DEPLOY_ROOT%\backups
set TIMESTAMP=%date:~10,4%-%date:~4,2%-%date:~7,2%_%time:~0,2%-%time:~3,2%-%time:~6,2%
set TIMESTAMP=%TIMESTAMP: =0%

echo Source: %FRONTEND_SOURCE%
echo Deploy: %FRONTEND_DEPLOY%
echo.

REM Verify source directory exists
if not exist "%FRONTEND_SOURCE%" (
    echo ERROR: Frontend source directory not found: %FRONTEND_SOURCE%
    exit /b 1
)

REM Create deployment directories
echo [1/7] Creating deployment directories...
if not exist "%DEPLOY_ROOT%" mkdir "%DEPLOY_ROOT%"
if not exist "%BACKUP_ROOT%" mkdir "%BACKUP_ROOT%"

REM Backup existing deployment
if exist "%FRONTEND_DEPLOY%" (
    echo [2/7] Backing up existing deployment...
    set BACKUP_DIR=%BACKUP_ROOT%\frontend_%TIMESTAMP%
    mkdir "!BACKUP_DIR!"
    xcopy "%FRONTEND_DEPLOY%" "!BACKUP_DIR!" /E /I /Q /EXCLUDE:exclude_patterns.txt 2>nul
    echo Backup created: !BACKUP_DIR!
) else (
    echo [2/7] No existing deployment to backup
)

echo.
echo [3/7] Installing dependencies...
cd /d "%FRONTEND_SOURCE%"

REM Clean node_modules and reinstall
if exist "node_modules" rmdir /s /q "node_modules"
call npm install

if %errorLevel% neq 0 (
    echo ERROR: npm install failed!
    exit /b 1
)

echo.
echo [4/7] Building Next.js application...

REM Set production environment
set NODE_ENV=production

REM Clean previous build
if exist ".next" rmdir /s /q ".next"

REM Build Next.js
call npm run build

if %errorLevel% neq 0 (
    echo ERROR: Build failed!
    exit /b 1
)

echo.
echo [5/7] Stopping PM2 services...

REM Stop existing PM2 process
call pm2 delete vuteq-frontend 2>nul
call pm2 save --force

echo.
echo [6/7] Deploying to %FRONTEND_DEPLOY%...

REM Remove old deployment (except node_modules for faster deploy)
if exist "%FRONTEND_DEPLOY%" (
    if exist "%FRONTEND_DEPLOY%\node_modules" (
        echo Preserving node_modules...
        move "%FRONTEND_DEPLOY%\node_modules" "%TEMP%\vuteq_node_modules" >nul 2>&1
    )
    rmdir /s /q "%FRONTEND_DEPLOY%"
)

REM Create deployment directory
mkdir "%FRONTEND_DEPLOY%"

REM Copy all files except development artifacts
REM Using robocopy for reliable copying of all files including BUILD_ID
if exist "%~dp0exclude_frontend.txt" (
    robocopy "%FRONTEND_SOURCE%" "%FRONTEND_DEPLOY%" /E /COPYALL /R:3 /W:5 /XF exclude_frontend.txt /NFL /NDL /NJH /NJS
) else (
    REM Copy everything if exclude file doesn't exist - ensures BUILD_ID and all .next files are copied
    robocopy "%FRONTEND_SOURCE%" "%FRONTEND_DEPLOY%" /E /COPYALL /R:3 /W:5 /NFL /NDL /NJH /NJS
)
REM robocopy returns 0-7 for success (0=no changes, 1=files copied, 2=extra files, etc.)
if %errorLevel% geq 8 (
    echo ERROR: File copy failed!
    exit /b 1
)

REM Restore node_modules if preserved
if exist "%TEMP%\vuteq_node_modules" (
    echo Restoring node_modules...
    move "%TEMP%\vuteq_node_modules" "%FRONTEND_DEPLOY%\node_modules" >nul 2>&1
) else (
    REM Install production dependencies in deployment
    cd /d "%FRONTEND_DEPLOY%"
    call npm install --production
)

REM Create production environment file
echo.
echo Creating production environment configuration...
(
echo NODE_ENV=production
echo NEXT_PUBLIC_API_URL=http://localhost
echo PORT=3000
echo HOSTNAME=localhost
) > "%FRONTEND_DEPLOY%\.env.production"

echo.
echo [7/7] Configuring PM2...

REM Create PM2 ecosystem file
(
echo module.exports = {
echo   apps: [{
echo     name: 'vuteq-frontend',
echo     cwd: 'E:\\VuteqDeploy\\frontend',
echo     script: 'node_modules\\next\\dist\\bin\\next',
echo     args: 'start -p 3000',
echo     instances: 1,
echo     exec_mode: 'fork',
echo     watch: false,
echo     env: {
echo       NODE_ENV: 'production',
echo       PORT: 3000,
echo       HOSTNAME: 'localhost'
echo     },
echo     error_file: 'E:\\VuteqDeploy\\logs\\frontend\\frontend-error.log',
echo     out_file: 'E:\\VuteqDeploy\\logs\\frontend\\frontend-out.log',
echo     log_date_format: 'YYYY-MM-DD HH:mm:ss Z',
echo     merge_logs: true,
echo     autorestart: true,
echo     max_restarts: 10,
echo     min_uptime: '10s',
echo     max_memory_restart: '1G'
echo   }]
echo };
) > "%FRONTEND_DEPLOY%\ecosystem.config.js"

REM Create logs directory
if not exist "E:\VuteqDeploy\logs\frontend" mkdir "E:\VuteqDeploy\logs\frontend"

REM Set folder permissions
echo Setting folder permissions...
icacls "%FRONTEND_DEPLOY%" /grant "IIS_IUSRS:(OI)(CI)F" /T /Q
icacls "E:\VuteqDeploy\logs" /grant "IIS_IUSRS:(OI)(CI)F" /T /Q

if "%NOPAUSE_MODE%"=="0" (
    echo.
    echo ============================================================================
    echo Frontend Deployment Complete!
    echo ============================================================================
    echo.
    echo Deployed to: %FRONTEND_DEPLOY%
    echo Configuration: %FRONTEND_DEPLOY%\ecosystem.config.js
    echo Environment: %FRONTEND_DEPLOY%\.env.production
    echo Logs: E:\VuteqDeploy\logs\frontend\frontend-*.log
    echo.
    echo Next steps:
    echo   1. Run configure-iis.bat (if not done already)
    echo   2. Run start-services.bat
    echo.
    echo To start the frontend now, run:
    echo   pm2 start "%FRONTEND_DEPLOY%\ecosystem.config.js"
    echo   pm2 save
    echo.
) else (
    echo Frontend deployed successfully
)

REM Return to original directory
cd /d "%SCRIPT_DIR%"

endlocal
exit /b 0
