param(
  [string]$GatewayBaseUrl = "http://localhost:5030",
  [string]$AuthDbConnectionString = "Server=localhost,14333;Database=CRM.AuthDb;User Id=sa;Password=NetMetric.Dev.Sql.2026!;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True",
  [string]$AccountDbConnectionString = "Server=localhost,14333;Database=CRM.AccountDb;User Id=sa;Password=NetMetric.Dev.Sql.2026!;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True",
  [switch]$CiOptional,
  [switch]$NoDbTokenSeed,
  [int]$TimeoutSeconds = 30
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$project = Join-Path $repoRoot "tools\NetMetric.LocalSmoke\NetMetric.LocalSmoke.csproj"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
  throw "Required command not found: dotnet"
}

function Test-EndpointReady {
  param(
    [Parameter(Mandatory = $true)][string]$Url,
    [int]$Timeout = 10
  )

  try {
    $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec $Timeout -MaximumRedirection 0
    return $response.StatusCode -ge 200 -and $response.StatusCode -lt 400
  }
  catch {
    return $false
  }
}

$requiredEndpoints = @(
  "$GatewayBaseUrl/health/ready",
  "$GatewayBaseUrl/auth/health/ready",
  "$GatewayBaseUrl/account/health/ready"
)

$notReady = @()
foreach ($endpoint in $requiredEndpoints) {
  if (-not (Test-EndpointReady -Url $endpoint -Timeout $TimeoutSeconds)) {
    $notReady += $endpoint
  }
}

if ($notReady.Count -gt 0) {
  $joined = $notReady -join ", "
  throw "Local smoke prerequisites are not ready. Unreachable readiness endpoints: $joined. Start local stack via 'pnpm run dev:up' and retry."
}

$args = @(
  "run",
  "--project",
  $project,
  "--",
  "--gateway",
  $GatewayBaseUrl,
  "--timeout-seconds",
  $TimeoutSeconds
)

if (-not $NoDbTokenSeed) {
  $args += @("--auth-db", $AuthDbConnectionString)
}
else {
  $args += "--no-db-token-seed"
}

if (-not [string]::IsNullOrWhiteSpace($AccountDbConnectionString)) {
  $args += @("--account-db", $AccountDbConnectionString)
}

if ($CiOptional) {
  $args += "--ci-optional"
}

Push-Location $repoRoot
try {
  & dotnet @args
  exit $LASTEXITCODE
}
finally {
  Pop-Location
}
