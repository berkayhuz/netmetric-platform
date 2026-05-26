[CmdletBinding()]
param(
  [string]$BaseDomain = "netmetric.net",
  [string]$Scheme = "https",
  [switch]$IncludeAuthenticated
)

$ErrorActionPreference = "Stop"

function Test-Endpoint {
  param(
    [Parameter(Mandatory = $true)][string]$Name,
    [Parameter(Mandatory = $true)][string]$Url
  )

  try {
    $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 20 -MaximumRedirection 0
    Write-Host ("[PASS] {0} -> HTTP {1} ({2})" -f $Name, [int]$response.StatusCode, $Url) -ForegroundColor Green
    return $true
  }
  catch {
    if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
      $statusCode = [int]$_.Exception.Response.StatusCode
      if ($statusCode -ge 300 -and $statusCode -lt 400) {
        Write-Host ("[PASS] {0} -> Redirect {1} ({2})" -f $Name, $statusCode, $Url) -ForegroundColor Green
        return $true
      }
      Write-Host ("[FAIL] {0} -> HTTP {1} ({2})" -f $Name, $statusCode, $Url) -ForegroundColor Red
      return $false
    }

    Write-Host ("[FAIL] {0} -> {1}" -f $Name, $_.Exception.Message) -ForegroundColor Red
    return $false
  }
}

$publicHost = $BaseDomain
$accountHost = "account.$BaseDomain"
$toolsHost = "tools.$BaseDomain"
$crmHost = "crm.$BaseDomain"
$authHost = "auth.$BaseDomain"

$checks = @(
  @{ Name = "public-root"; Url = "$Scheme://$publicHost/" },
  @{ Name = "public-live"; Url = "$Scheme://$publicHost/health/live" },
  @{ Name = "public-ready"; Url = "$Scheme://$publicHost/health/ready" },
  @{ Name = "public-robots"; Url = "$Scheme://$publicHost/robots.txt" },
  @{ Name = "public-sitemap"; Url = "$Scheme://$publicHost/sitemap.xml" },
  @{ Name = "account-live"; Url = "$Scheme://$accountHost/health/live" },
  @{ Name = "account-ready"; Url = "$Scheme://$accountHost/health/ready" },
  @{ Name = "account-protected"; Url = "$Scheme://$accountHost/" },
  @{ Name = "tools-live"; Url = "$Scheme://$toolsHost/health/live" },
  @{ Name = "tools-ready"; Url = "$Scheme://$toolsHost/health/ready" },
  @{ Name = "crm-live"; Url = "$Scheme://$crmHost/health/live" },
  @{ Name = "crm-ready"; Url = "$Scheme://$crmHost/health/ready" },
  @{ Name = "crm-protected"; Url = "$Scheme://$crmHost/dashboard" },
  @{ Name = "auth-login"; Url = "$Scheme://$authHost/login" }
)

$hasFail = $false
Write-Host "=== Domain Smoke ($Scheme://*.$BaseDomain) ==="
foreach ($check in $checks) {
  $ok = Test-Endpoint -Name $check.Name -Url $check.Url
  if (-not $ok) { $hasFail = $true }
}

if ($IncludeAuthenticated) {
  Write-Host ""
  Write-Host "Authenticated smoke remains manual (credentials are not handled by this script):" -ForegroundColor Yellow
  Write-Host "- account: /profile /preferences /security /notifications /settings /settings/team"
  Write-Host "- tools: history save/download/delete"
  Write-Host "- crm: dashboard + customers/companies/contacts + address mutations"
}

if ($hasFail) {
  Write-Host "Domain smoke FAILED." -ForegroundColor Red
  exit 1
}

Write-Host "Domain smoke PASSED." -ForegroundColor Green
exit 0
