[CmdletBinding()]
param(
  [string]$TagSuffix = "smoke"
)

$ErrorActionPreference = "Stop"

function Test-DockerAvailable {
  try {
    $null = Get-Command docker -ErrorAction Stop
    $null = docker version --format "{{.Server.Version}}" 2>$null
    return $true
  }
  catch {
    return $false
  }
}

if (-not (Test-DockerAvailable)) {
  Write-Host "[FAIL] Docker is not available or Docker daemon is not reachable." -ForegroundColor Red
  exit 1
}

$builds = @(
  @{
    Name = "public-web"
    Dockerfile = "apps/public-web/Dockerfile"
    Tag = "ghcr.io/netmetric/public-web:$TagSuffix"
    Args = @(
      "NEXT_PUBLIC_SITE_URL=https://netmetric.net"
      "NEXT_PUBLIC_AUTH_URL=https://auth.netmetric.net"
      "NEXT_PUBLIC_ACCOUNT_URL=https://account.netmetric.net"
      "NEXT_PUBLIC_CRM_URL=https://crm.netmetric.net"
      "NEXT_PUBLIC_TOOLS_URL=https://tools.netmetric.net"
      "NEXT_PUBLIC_API_URL=https://api.netmetric.net"
    )
  }
  @{
    Name = "account-web"
    Dockerfile = "apps/account-web/Dockerfile"
    Tag = "ghcr.io/netmetric/account-web:$TagSuffix"
    Args = @(
      "NEXT_PUBLIC_APP_NAME=NetMetric Account"
      "NEXT_PUBLIC_APP_ORIGIN=https://account.netmetric.net"
      "NEXT_PUBLIC_ACCOUNT_URL=https://account.netmetric.net"
      "NEXT_PUBLIC_AUTH_URL=https://auth.netmetric.net"
      "NEXT_PUBLIC_API_BASE_URL=https://api.netmetric.net"
    )
  }
  @{
    Name = "tools-web"
    Dockerfile = "apps/tools-web/Dockerfile"
    Tag = "ghcr.io/netmetric/tools-web:$TagSuffix"
    Args = @(
      "NEXT_PUBLIC_APP_NAME=NetMetric Tools"
      "NEXT_PUBLIC_APP_ORIGIN=https://tools.netmetric.net"
      "NEXT_PUBLIC_TOOLS_URL=https://tools.netmetric.net"
      "NEXT_PUBLIC_AUTH_URL=https://auth.netmetric.net"
      "NEXT_PUBLIC_API_BASE_URL=https://api.netmetric.net"
    )
  }
  @{
    Name = "tools-api"
    Dockerfile = "services/tools/src/NetMetric.Tools.API/Dockerfile"
    Tag = "ghcr.io/netmetric/tools-api:$TagSuffix"
    Args = @()
  }
  @{
    Name = "crm-web"
    Dockerfile = "apps/crm-web/Dockerfile"
    Tag = "ghcr.io/netmetric/crm-web:$TagSuffix"
    Args = @(
      "NEXT_PUBLIC_APP_NAME=NetMetric CRM"
      "NEXT_PUBLIC_APP_ORIGIN=https://crm.netmetric.net"
      "NEXT_PUBLIC_CRM_URL=https://crm.netmetric.net"
      "NEXT_PUBLIC_AUTH_URL=https://auth.netmetric.net"
      "NEXT_PUBLIC_API_BASE_URL=https://api.netmetric.net"
    )
  }
)

$hasFail = $false

foreach ($build in $builds) {
  Write-Host "=== Building $($build.Name) ==="
  $argList = @("build", "-f", $build.Dockerfile, "-t", $build.Tag)
  foreach ($arg in $build.Args) {
    $argList += @("--build-arg", $arg)
  }
  $argList += "."

  & docker @argList
  if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] $($build.Name) image build failed." -ForegroundColor Red
    $hasFail = $true
    continue
  }

  Write-Host "[PASS] $($build.Name) -> $($build.Tag)" -ForegroundColor Green
}

if ($hasFail) {
  Write-Host "Docker image smoke FAILED." -ForegroundColor Red
  exit 1
}

Write-Host "Docker image smoke PASSED." -ForegroundColor Green
exit 0
