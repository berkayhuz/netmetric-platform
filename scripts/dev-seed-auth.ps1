param(
  [string]$Email = "owner@netmetric.local",
  [string]$Password = "",
  [string]$UserName = "netmetric-owner",
  [string]$TenantName = "NetMetric Dev Tenant"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "dev\common.ps1")

$seedPassword = if ([string]::IsNullOrWhiteSpace($Password)) {
  $env:NETMETRIC_DEV_SEED_PASSWORD
} else {
  $Password
}

if ([string]::IsNullOrWhiteSpace($seedPassword)) {
  throw "Set NETMETRIC_DEV_SEED_PASSWORD or pass -Password before running the auth seed script."
}

$gatewayBaseUrl = "http://localhost:5030"
$registerUrl = "$gatewayBaseUrl/api/auth/register"

Write-Log "Checking gateway readiness."
Wait-HttpOk -Url "$gatewayBaseUrl/health/ready" -TimeoutSeconds 120

$payload = @{
  userName   = $UserName
  email      = $Email
  password   = $seedPassword
  tenantName = $TenantName
} | ConvertTo-Json -Depth 5

Write-Log "Sending auth seed register request via API gateway."

try {
  $response = Invoke-WebRequest `
    -Uri $registerUrl `
    -Method Post `
    -ContentType "application/json" `
    -Body $payload `
    -UseBasicParsing

  Write-Log "Seed request completed with status $($response.StatusCode)."
}
catch {
  if ($_.Exception.Response -ne $null) {
    $reader = New-Object IO.StreamReader($_.Exception.Response.GetResponseStream())
    $body = $reader.ReadToEnd()
    $reader.Dispose()

    Write-Log "Seed request failed: $body"
  }

  throw
}
