# ============================================================================
# VUTEQ Scanner - IIS Configuration Script
# Author: Hassan
# Date: 2026-01-07
# Description: Configures IIS sites, app pools, and reverse proxy rules
# ============================================================================

Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "VUTEQ Scanner - Configuring IIS" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""

# Check for administrator privileges
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as administrator'" -ForegroundColor Yellow
    pause
    exit 1
}

# Import IIS module
Import-Module WebAdministration -ErrorAction SilentlyContinue
if (-not (Get-Module WebAdministration)) {
    Write-Host "ERROR: IIS WebAdministration module not available!" -ForegroundColor Red
    Write-Host "Please ensure IIS is installed with management tools" -ForegroundColor Yellow
    pause
    exit 1
}

# Configuration
$siteName = "VUTEQ Scanner"
$backendPool = "VuteqBackendPool"
$sitePort = 80
$backendPath = "C:\inetpub\vuteq\backend"
$backendUrl = "http://localhost:5000"
$frontendUrl = "http://localhost:3000"

Write-Host "[1/8] Creating Application Pool for Backend..." -ForegroundColor Green

# Remove existing app pool if exists
if (Test-Path "IIS:\AppPools\$backendPool") {
    Write-Host "  Removing existing app pool..." -ForegroundColor Yellow
    Remove-WebAppPool -Name $backendPool
}

# Create new app pool
New-WebAppPool -Name $backendPool
Set-ItemProperty "IIS:\AppPools\$backendPool" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\$backendPool" -Name "startMode" -Value "AlwaysRunning"
Set-ItemProperty "IIS:\AppPools\$backendPool" -Name "processModel.idleTimeout" -Value "00:00:00"
Set-ItemProperty "IIS:\AppPools\$backendPool" -Name "recycling.periodicRestart.time" -Value "00:00:00"
Set-ItemProperty "IIS:\AppPools\$backendPool" -Name "processModel.loadUserProfile" -Value $true

Write-Host "  Application pool '$backendPool' created" -ForegroundColor Gray

Write-Host ""
Write-Host "[2/8] Removing existing site if present..." -ForegroundColor Green

# Stop and remove existing site
if (Test-Path "IIS:\Sites\$siteName") {
    Write-Host "  Stopping existing site..." -ForegroundColor Yellow
    Stop-WebSite -Name $siteName -ErrorAction SilentlyContinue
    Remove-WebSite -Name $siteName
}

Write-Host ""
Write-Host "[3/8] Creating IIS Site..." -ForegroundColor Green

# Create new site
New-WebSite -Name $siteName `
    -Port $sitePort `
    -PhysicalPath $backendPath `
    -ApplicationPool $backendPool `
    -Force

Write-Host "  Site '$siteName' created on port $sitePort" -ForegroundColor Gray

Write-Host ""
Write-Host "[4/8] Enabling ARR Proxy..." -ForegroundColor Green

# Enable ARR proxy
$arrConfig = Get-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" -Filter "system.webServer/proxy" -Name "enabled" -ErrorAction SilentlyContinue
if ($null -ne $arrConfig) {
    Set-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" -Filter "system.webServer/proxy" -Name "enabled" -Value $true
    Set-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" -Filter "system.webServer/proxy" -Name "preserveHostHeader" -Value $true
    Set-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" -Filter "system.webServer/proxy" -Name "reverseRewriteHostInResponseHeaders" -Value $false
    Write-Host "  ARR proxy enabled" -ForegroundColor Gray
} else {
    Write-Host "  WARNING: ARR not detected. Please install Application Request Routing" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "[5/8] Configuring URL Rewrite Rules..." -ForegroundColor Green

# Create web.config with rewrite rules
$webConfigPath = Join-Path $backendPath "web.config"
$webConfigContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet"
                arguments=".\VuteqScanner.dll"
                stdoutLogEnabled="true"
                stdoutLogFile=".\logs\stdout"
                hostingModel="inprocess">
      <environmentVariables>
        <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
      </environmentVariables>
    </aspNetCore>
    <rewrite>
      <rules>
        <!-- API Backend Proxy Rule -->
        <rule name="API Reverse Proxy" stopProcessing="true">
          <match url="^api/(.*)" />
          <action type="Rewrite" url="http://localhost:5000/api/{R:1}" />
          <serverVariables>
            <set name="HTTP_X_ORIGINAL_HOST" value="{HTTP_HOST}" />
            <set name="HTTP_X_FORWARDED_FOR" value="{REMOTE_ADDR}" />
          </serverVariables>
        </rule>

        <!-- Frontend Proxy Rule for static files -->
        <rule name="Frontend Static Files" stopProcessing="true">
          <match url="^(_next|static|images|favicon\.ico|robots\.txt)(.*)" />
          <action type="Rewrite" url="http://localhost:3000/{R:0}" />
        </rule>

        <!-- Frontend Proxy Rule for all other requests -->
        <rule name="Frontend Reverse Proxy" stopProcessing="true">
          <match url="(.*)" />
          <conditions>
            <add input="{REQUEST_URI}" pattern="^/api/" negate="true" />
          </conditions>
          <action type="Rewrite" url="http://localhost:3000/{R:1}" />
          <serverVariables>
            <set name="HTTP_X_ORIGINAL_HOST" value="{HTTP_HOST}" />
            <set name="HTTP_X_FORWARDED_FOR" value="{REMOTE_ADDR}" />
          </serverVariables>
        </rule>
      </rules>
      <outboundRules>
        <rule name="Add HSTS Header" preCondition="HTTPS">
          <match serverVariable="RESPONSE_Strict-Transport-Security" pattern=".*" />
          <action type="Rewrite" value="max-age=31536000; includeSubDomains" />
        </rule>
        <preConditions>
          <preCondition name="HTTPS">
            <add input="{HTTPS}" pattern="on" />
          </preCondition>
        </preConditions>
      </outboundRules>
    </rewrite>
    <httpErrors errorMode="Detailed" />
    <security>
      <requestFiltering>
        <requestLimits maxAllowedContentLength="104857600" />
      </requestFiltering>
    </security>
  </system.webServer>
</configuration>
"@

Set-Content -Path $webConfigPath -Value $webConfigContent -Force
Write-Host "  web.config created with rewrite rules" -ForegroundColor Gray

Write-Host ""
Write-Host "[6/8] Configuring Server Variables..." -ForegroundColor Green

# Allow server variables for proxy
try {
    Add-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" `
        -Filter "system.webServer/rewrite/allowedServerVariables" `
        -Name "." `
        -Value @{name='HTTP_X_ORIGINAL_HOST'} `
        -ErrorAction SilentlyContinue

    Add-WebConfigurationProperty -PSPath "MACHINE/WEBROOT/APPHOST" `
        -Filter "system.webServer/rewrite/allowedServerVariables" `
        -Name "." `
        -Value @{name='HTTP_X_FORWARDED_FOR'} `
        -ErrorAction SilentlyContinue

    Write-Host "  Server variables configured" -ForegroundColor Gray
} catch {
    Write-Host "  Server variables may already exist" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "[7/8] Setting Firewall Rules..." -ForegroundColor Green

# Allow HTTP traffic
New-NetFirewallRule -DisplayName "VUTEQ Scanner HTTP" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort 80 `
    -Action Allow `
    -ErrorAction SilentlyContinue

Write-Host "  Firewall rule created for port 80" -ForegroundColor Gray

Write-Host ""
Write-Host "[8/8] Starting IIS Site..." -ForegroundColor Green

# Start the site
Start-WebSite -Name $siteName

# Wait for site to start
Start-Sleep -Seconds 2

$siteState = (Get-WebSite -Name $siteName).State
Write-Host "  Site state: $siteState" -ForegroundColor Gray

Write-Host ""
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "IIS Configuration Complete!" -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Site Name: $siteName" -ForegroundColor White
Write-Host "Port: $sitePort" -ForegroundColor White
Write-Host "Backend Path: $backendPath" -ForegroundColor White
Write-Host "Application Pool: $backendPool" -ForegroundColor White
Write-Host ""
Write-Host "Routing:" -ForegroundColor Yellow
Write-Host "  http://localhost/api/* -> Backend ($backendUrl)" -ForegroundColor Gray
Write-Host "  http://localhost/* -> Frontend ($frontendUrl)" -ForegroundColor Gray
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Ensure SQL Server is running" -ForegroundColor White
Write-Host "  2. Run start-services.bat to start PM2 frontend" -ForegroundColor White
Write-Host "  3. Test the application: http://localhost" -ForegroundColor White
Write-Host ""

# Display site status
Write-Host "Current Site Status:" -ForegroundColor Yellow
Get-WebSite -Name $siteName | Format-Table Name, State, PhysicalPath, Bindings -AutoSize

pause
