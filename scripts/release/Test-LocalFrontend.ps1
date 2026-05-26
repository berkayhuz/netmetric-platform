[CmdletBinding()]
param(
  [switch]$Start,
  [switch]$SkipStart,
  [int]$TimeoutSeconds = 10
)

$ErrorActionPreference = "Stop"

function Test-Url {
  param(
    [Parameter(Mandatory = $true)][string]$Url,
    [int]$Timeout = 10
  )

  try {
    $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec $Timeout -MaximumRedirection 0
    return @{ Status = "PASS"; Code = $response.StatusCode; Detail = "HTTP $($response.StatusCode)" }
  }
  catch {
    $message = $_.Exception.Message
    if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
      $statusCode = [int]$_.Exception.Response.StatusCode
      if ($statusCode -ge 300 -and $statusCode -lt 400) {
        return @{ Status = "PASS"; Code = $statusCode; Detail = "Redirect (expected for protected routes)" }
      }
      return @{ Status = "FAIL"; Code = $statusCode; Detail = $message }
    }

    return @{ Status = "FAIL"; Code = 0; Detail = $message }
  }
}

Write-Host "=== Local Frontend Smoke ==="
Write-Host "Start mode: $Start | SkipStart: $SkipStart | TimeoutSeconds: $TimeoutSeconds"

if ($Start -and -not $SkipStart) {
  Write-Warning "Auto-start lifecycle is intentionally not managed in this script to avoid orphan dev processes."
  Write-Host "Run 'pnpm frontend:start' in a separate terminal, then rerun this script with -SkipStart."
  exit 2
}

if (-not $Start -and -not $SkipStart) {
  Write-Host "Tip: if apps are not running, start them manually with: pnpm frontend:start"
}

$checks = @(
  @{ Name = "auth-web"; Url = "http://localhost:7002" },
  @{ Name = "public-web"; Url = "http://localhost:7003" },
  @{ Name = "account-web"; Url = "http://localhost:7004" },
  @{ Name = "tools-web"; Url = "http://localhost:7005" },
  @{ Name = "crm-web"; Url = "http://localhost:7006" },
  @{ Name = "account-live"; Url = "http://localhost:7004/health/live" },
  @{ Name = "account-ready"; Url = "http://localhost:7004/health/ready" },
  @{ Name = "tools-live"; Url = "http://localhost:7005/health/live" },
  @{ Name = "tools-ready"; Url = "http://localhost:7005/health/ready" },
  @{ Name = "crm-live"; Url = "http://localhost:7006/health/live" },
  @{ Name = "crm-ready"; Url = "http://localhost:7006/health/ready" }
)

$hasFail = $false
foreach ($check in $checks) {
  $result = Test-Url -Url $check.Url -Timeout $TimeoutSeconds
  $line = "[{0}] {1} -> {2} ({3})" -f $result.Status, $check.Name, $check.Url, $result.Detail
  if ($result.Status -eq "FAIL") {
    $hasFail = $true
    Write-Host $line -ForegroundColor Red
  }
  else {
    Write-Host $line -ForegroundColor Green
  }
}

if ($hasFail) {
  Write-Host "Local frontend smoke FAILED." -ForegroundColor Red
  exit 1
}

Write-Host "Local frontend smoke PASSED." -ForegroundColor Green
exit 0
