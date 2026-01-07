@echo off
REM ============================================================================
REM VUTEQ Scanner - IIS Prerequisites Installation Script
REM Author: Hassan
REM Date: 2026-01-07
REM Description: Installs all required prerequisites for IIS deployment
REM ============================================================================

echo ============================================================================
echo VUTEQ Scanner - Installing Prerequisites
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

echo [1/6] Enabling IIS and required features...
echo.

REM Enable IIS with all required features
dism /online /enable-feature /featurename:IIS-WebServerRole /all
dism /online /enable-feature /featurename:IIS-WebServer /all
dism /online /enable-feature /featurename:IIS-CommonHttpFeatures /all
dism /online /enable-feature /featurename:IIS-HttpErrors /all
dism /online /enable-feature /featurename:IIS-HttpRedirect /all
dism /online /enable-feature /featurename:IIS-ApplicationDevelopment /all
dism /online /enable-feature /featurename:IIS-NetFxExtensibility45 /all
dism /online /enable-feature /featurename:IIS-HealthAndDiagnostics /all
dism /online /enable-feature /featurename:IIS-HttpLogging /all
dism /online /enable-feature /featurename:IIS-LoggingLibraries /all
dism /online /enable-feature /featurename:IIS-RequestMonitor /all
dism /online /enable-feature /featurename:IIS-HttpTracing /all
dism /online /enable-feature /featurename:IIS-Security /all
dism /online /enable-feature /featurename:IIS-RequestFiltering /all
dism /online /enable-feature /featurename:IIS-Performance /all
dism /online /enable-feature /featurename:IIS-WebServerManagementTools /all
dism /online /enable-feature /featurename:IIS-IIS6ManagementCompatibility /all
dism /online /enable-feature /featurename:IIS-Metabase /all
dism /online /enable-feature /featurename:IIS-ManagementConsole /all
dism /online /enable-feature /featurename:IIS-BasicAuthentication /all
dism /online /enable-feature /featurename:IIS-WindowsAuthentication /all
dism /online /enable-feature /featurename:IIS-StaticContent /all
dism /online /enable-feature /featurename:IIS-DefaultDocument /all
dism /online /enable-feature /featurename:IIS-DirectoryBrowsing /all
dism /online /enable-feature /featurename:IIS-ASPNET45 /all
dism /online /enable-feature /featurename:IIS-ISAPIExtensions /all
dism /online /enable-feature /featurename:IIS-ISAPIFilter /all
dism /online /enable-feature /featurename:IIS-HttpCompressionStatic /all
dism /online /enable-feature /featurename:IIS-HttpCompressionDynamic /all

echo.
echo [2/6] Installing .NET 8.0 Hosting Bundle...
echo.

REM Download .NET 8.0 Hosting Bundle
set DOTNET_INSTALLER=%TEMP%\dotnet-hosting-8.0-win.exe
echo Downloading .NET 8.0 Hosting Bundle...
powershell -Command "& {Invoke-WebRequest -Uri 'https://download.visualstudio.microsoft.com/download/pr/e2fa8b9e-2989-47ee-9f80-c18c6bc453f2/67fa5ce45d92a49b6c8bc6c3e235dbfc/dotnet-hosting-8.0.1-win.exe' -OutFile '%DOTNET_INSTALLER%'}"

if exist "%DOTNET_INSTALLER%" (
    echo Installing .NET 8.0 Hosting Bundle...
    "%DOTNET_INSTALLER%" /quiet /norestart
    echo .NET 8.0 Hosting Bundle installed successfully
    del "%DOTNET_INSTALLER%"
) else (
    echo WARNING: Failed to download .NET 8.0 Hosting Bundle
    echo Please download and install manually from: https://dotnet.microsoft.com/download/dotnet/8.0
)

echo.
echo [3/6] Installing Node.js 20 LTS...
echo.

REM Download Node.js 20 LTS
set NODE_INSTALLER=%TEMP%\node-v20-x64.msi
echo Downloading Node.js 20 LTS...
powershell -Command "& {Invoke-WebRequest -Uri 'https://nodejs.org/dist/v20.11.0/node-v20.11.0-x64.msi' -OutFile '%NODE_INSTALLER%'}"

if exist "%NODE_INSTALLER%" (
    echo Installing Node.js 20 LTS...
    msiexec /i "%NODE_INSTALLER%" /quiet /norestart
    echo Node.js 20 LTS installed successfully
    del "%NODE_INSTALLER%"
) else (
    echo WARNING: Failed to download Node.js
    echo Please download and install manually from: https://nodejs.org/
)

echo.
echo [4/6] Installing URL Rewrite Module...
echo.

REM Download and install URL Rewrite Module
set URL_REWRITE_INSTALLER=%TEMP%\rewrite_amd64.msi
echo Downloading URL Rewrite Module...
powershell -Command "& {Invoke-WebRequest -Uri 'https://download.microsoft.com/download/1/2/8/128E2E22-C1B9-44A4-BE2A-5859ED1D4592/rewrite_amd64_en-US.msi' -OutFile '%URL_REWRITE_INSTALLER%'}"

if exist "%URL_REWRITE_INSTALLER%" (
    echo Installing URL Rewrite Module...
    msiexec /i "%URL_REWRITE_INSTALLER%" /quiet /norestart
    echo URL Rewrite Module installed successfully
    del "%URL_REWRITE_INSTALLER%"
) else (
    echo WARNING: Failed to download URL Rewrite Module
    echo Please download and install manually from IIS downloads
)

echo.
echo [5/6] Installing Application Request Routing (ARR)...
echo.

REM Download and install ARR
set ARR_INSTALLER=%TEMP%\arr_amd64.msi
echo Downloading Application Request Routing...
powershell -Command "& {Invoke-WebRequest -Uri 'https://download.microsoft.com/download/E/9/8/E9849D6A-020E-47E4-9FD0-A023E99B54EB/requestRouter_amd64.msi' -OutFile '%ARR_INSTALLER%'}"

if exist "%ARR_INSTALLER%" (
    echo Installing Application Request Routing...
    msiexec /i "%ARR_INSTALLER%" /quiet /norestart
    echo Application Request Routing installed successfully
    del "%ARR_INSTALLER%"
) else (
    echo WARNING: Failed to download ARR
    echo Please download and install manually from IIS downloads
)

echo.
echo [6/6] Installing PM2 Process Manager...
echo.

REM Refresh environment variables to get Node.js path
call refreshenv >nul 2>&1
setlocal enabledelayedexpansion
set "PATH=%PATH%;%ProgramFiles%\nodejs"

REM Install PM2 globally
echo Installing PM2 globally via npm...
call npm install -g pm2
call npm install -g pm2-windows-startup

if %errorLevel% equ 0 (
    echo PM2 installed successfully

    REM Setup PM2 startup
    echo Configuring PM2 to run on system startup...
    call pm2-startup install
) else (
    echo WARNING: Failed to install PM2
    echo You may need to run: npm install -g pm2
)

echo.
echo ============================================================================
echo Prerequisites Installation Complete!
echo ============================================================================
echo.
echo IMPORTANT: Please restart the server for all changes to take effect.
echo.
echo After restart, verify installations:
echo   - IIS: Open IIS Manager
echo   - .NET 8: Run 'dotnet --info'
echo   - Node.js: Run 'node --version'
echo   - PM2: Run 'pm2 --version'
echo.
echo Next steps:
echo   1. Restart the server
echo   2. Run deploy-backend.bat
echo   3. Run deploy-frontend.bat
echo   4. Run configure-iis.bat
echo   5. Run start-services.bat
echo.
pause
