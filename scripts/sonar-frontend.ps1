param(
    [string]$SonarUrl = "http://localhost:9000",
    [switch]$StrictCoverage
)

$ErrorActionPreference = "Stop"

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string]$StepName,

        [Parameter(Mandatory = $true)]
        [scriptblock]$Command
    )

    Write-Host ""
    Write-Host "==> $StepName"

    try {
        & $Command
    }
    catch {
        Write-Error "$StepName failed. $($_.Exception.Message)"
        exit 1
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Error "$StepName failed with exit code $LASTEXITCODE."
        exit $LASTEXITCODE
    }
}

function Test-PackageScriptExists {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Scripts,

        [Parameter(Mandatory = $true)]
        [string]$ScriptName
    )

    if ($null -eq $Scripts) {
        return $false
    }

    return $Scripts.PSObject.Properties.Name -contains $ScriptName
}

function Invoke-PnpmScriptIfExists {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Scripts,

        [Parameter(Mandatory = $true)]
        [string]$ScriptName,

        [switch]$ContinueOnFailure
    )

    if (-not (Test-PackageScriptExists -Scripts $Scripts -ScriptName $ScriptName)) {
        Write-Warning "package.json script '$ScriptName' not found. Skipping."
        return $false
    }

    Write-Host ""
    Write-Host "==> pnpm run $ScriptName"

    pnpm run $ScriptName

    if ($LASTEXITCODE -ne 0) {
        if ($ContinueOnFailure) {
            Write-Warning "pnpm run $ScriptName failed with exit code $LASTEXITCODE. Continuing."
            return $false
        }

        Write-Error "pnpm run $ScriptName failed with exit code $LASTEXITCODE."
        exit $LASTEXITCODE
    }

    return $true
}

if ([string]::IsNullOrWhiteSpace($env:SONAR_TOKEN)) {
    Write-Error "SONAR_TOKEN environment variable is required. Set it first: `$env:SONAR_TOKEN='your-token'"
    exit 1
}

if ($null -eq (Get-Command -Name "pnpm" -ErrorAction SilentlyContinue)) {
    Write-Error "pnpm was not found in PATH. Install pnpm and retry."
    exit 1
}

if (-not (Test-Path -LiteralPath ".\sonar-project.frontend.properties")) {
    Write-Error "sonar-project.frontend.properties not found at repository root."
    exit 1
}

if (-not (Test-Path -LiteralPath ".\package.json")) {
    Write-Error "package.json not found at repository root."
    exit 1
}

$package = Get-Content -Raw -LiteralPath ".\package.json" | ConvertFrom-Json
$packageScripts = $package.scripts

$ciFlag = [string]$env:CI
$strictCoverageMode = $StrictCoverage -or ($ciFlag -match '^(?i:true|1|yes)$')

if ($strictCoverageMode) {
    Write-Host "Strict coverage mode is enabled."
}
else {
    Write-Host "Local coverage mode is enabled."
}

Invoke-Checked -StepName "pnpm install --frozen-lockfile" -Command {
    pnpm install --frozen-lockfile
}

if (Test-PackageScriptExists -Scripts $packageScripts -ScriptName "frontend:check") {
    Invoke-PnpmScriptIfExists -Scripts $packageScripts -ScriptName "frontend:check" | Out-Null
}
else {
    if (Test-PackageScriptExists -Scripts $packageScripts -ScriptName "frontend:typecheck") {
        Invoke-PnpmScriptIfExists -Scripts $packageScripts -ScriptName "frontend:typecheck" | Out-Null
    }

    if (Test-PackageScriptExists -Scripts $packageScripts -ScriptName "frontend:lint") {
        Invoke-PnpmScriptIfExists -Scripts $packageScripts -ScriptName "frontend:lint" | Out-Null
    }

    if (Test-PackageScriptExists -Scripts $packageScripts -ScriptName "frontend:test") {
        Invoke-PnpmScriptIfExists -Scripts $packageScripts -ScriptName "frontend:test" | Out-Null
    }
}

$coverageRan = $false

if (Test-PackageScriptExists -Scripts $packageScripts -ScriptName "frontend:coverage") {
    $coverageRan = Invoke-PnpmScriptIfExists -Scripts $packageScripts -ScriptName "frontend:coverage" -ContinueOnFailure
}
elseif (Test-PackageScriptExists -Scripts $packageScripts -ScriptName "coverage") {
    $coverageRan = Invoke-PnpmScriptIfExists -Scripts $packageScripts -ScriptName "coverage" -ContinueOnFailure
}
else {
    if ($strictCoverageMode) {
        Write-Error "Strict coverage mode: no coverage script found in package.json."
        exit 1
    }

    Write-Warning "No coverage script found in package.json. Sonar analysis will continue without new LCOV generation."
}

$lcovPaths = @(
    ".\packages\frontend\ui\coverage\lcov.info",
    ".\packages\frontend\config\coverage\lcov.info"
)

$existingLcovPaths = $lcovPaths | Where-Object { Test-Path -LiteralPath $_ }

if ($strictCoverageMode -and $existingLcovPaths.Count -eq 0) {
    Write-Error "Strict coverage mode: no LCOV report found. Expected at least one of: $($lcovPaths -join ', ')"
    exit 1
}

if (-not $strictCoverageMode -and $existingLcovPaths.Count -eq 0) {
    Write-Warning "No LCOV report found. Sonar analysis will continue; frontend coverage may appear as 0%."
}

if (-not $coverageRan) {
    if ($strictCoverageMode) {
        Write-Error "Strict coverage mode: coverage script did not run successfully."
        exit 1
    }

    Write-Warning "Coverage step did not run successfully. Sonar analysis will continue; coverage may appear as 0%."
}

$env:SONAR_HOST_URL = $SonarUrl

Invoke-Checked -StepName "SonarScanner for NPM" -Command {
    node .\tools\sonar\scan-frontend.mjs
}

Write-Host ""
Write-Host "SonarQube frontend analysis completed successfully."