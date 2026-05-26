param(
    [string]$SonarUrl = "http://localhost:9000",
    [switch]$StrictCoverage
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($env:SONAR_TOKEN)) {
    Write-Error "SONAR_TOKEN environment variable is required. Set it first: `$env:SONAR_TOKEN='your-token'"
    exit 1
}

$backendScript = Join-Path $PSScriptRoot "sonar-backend.ps1"
$frontendScript = Join-Path $PSScriptRoot "sonar-frontend.ps1"

if (-not (Test-Path -LiteralPath $backendScript)) {
    Write-Error "Backend Sonar script not found: $backendScript"
    exit 1
}

if (-not (Test-Path -LiteralPath $frontendScript)) {
    Write-Error "Frontend Sonar script not found: $frontendScript"
    exit 1
}

Write-Host "==> Running backend SonarQube analysis"
& powershell -ExecutionPolicy Bypass -File $backendScript -SonarUrl $SonarUrl
if ($LASTEXITCODE -ne 0) {
    Write-Error "Backend SonarQube analysis failed with exit code $LASTEXITCODE."
    exit $LASTEXITCODE
}

Write-Host "==> Running frontend SonarQube analysis"
if ($StrictCoverage) {
    & powershell -ExecutionPolicy Bypass -File $frontendScript -SonarUrl $SonarUrl -StrictCoverage
}
else {
    & powershell -ExecutionPolicy Bypass -File $frontendScript -SonarUrl $SonarUrl
}
if ($LASTEXITCODE -ne 0) {
    Write-Error "Frontend SonarQube analysis failed with exit code $LASTEXITCODE."
    exit $LASTEXITCODE
}

Write-Host "All SonarQube analyses completed successfully."
