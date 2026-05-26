param(
  [string]$Email = "search-dev@netmetric.local",
  [string]$Password = $env:NETMETRIC_DEV_SEED_PASSWORD,
  [string]$UserName = "search-dev",
  [string]$TenantName = "NetMetric Search Dev Tenant",
  [string]$GatewayBaseUrl = "http://localhost:5030",
  [string]$OutputPath = "",
  [switch]$PrintToken,
  [switch]$LoginOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "common.ps1")

function Assert-LocalGatewayUrl {
  param([Parameter(Mandatory = $true)][string]$Url)

  $uri = [Uri]$Url
  $localHosts = @("localhost", "127.0.0.1", "::1")
  if ($uri.Scheme -ne "http" -or $uri.Host -notin $localHosts) {
    throw "Refusing to request auth tokens from a non-local gateway URL: $Url"
  }
}

function Get-SetCookieValues {
  param([Parameter(Mandatory = $true)]$Response)

  $headers = $Response.Headers
  if (-not $headers.ContainsKey("Set-Cookie")) {
    return @()
  }

  $value = $headers["Set-Cookie"]
  if ($value -is [array]) {
    return @($value)
  }

  return @([string]$value)
}

function Get-CookieValue {
  param(
    [Parameter(Mandatory = $true)][string[]]$SetCookieValues,
    [Parameter(Mandatory = $true)][string]$Name
  )

  foreach ($setCookie in $SetCookieValues) {
    $escapedName = [Regex]::Escape($Name)
    $match = [Regex]::Match($setCookie, "(^|,\s*)$escapedName=([^;,\s]+)")
    if ($match.Success) {
      return [Uri]::UnescapeDataString($match.Groups[2].Value)
    }
  }

  return $null
}

function ConvertFrom-Base64UrlJson {
  param([Parameter(Mandatory = $true)][string]$Value)

  $padded = $Value.Replace("-", "+").Replace("_", "/")
  switch ($padded.Length % 4) {
    2 { $padded += "==" }
    3 { $padded += "=" }
    0 { }
    default { throw "Invalid base64url payload length." }
  }

  $json = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($padded))
  return $json | ConvertFrom-Json
}

function Decode-JwtPayload {
  param([Parameter(Mandatory = $true)][string]$Token)

  $parts = $Token.Split(".")
  if ($parts.Count -lt 2) {
    throw "Access token is not a JWT."
  }

  return ConvertFrom-Base64UrlJson -Value $parts[1]
}

function Invoke-AuthRequest {
  param(
    [Parameter(Mandatory = $true)][string]$Path,
    [Parameter(Mandatory = $true)][hashtable]$Payload,
    [int[]]$AllowedStatusCodes = @(200)
  )

  $uri = "$($GatewayBaseUrl.TrimEnd('/'))/$($Path.TrimStart('/'))"
  $body = $Payload | ConvertTo-Json -Depth 8

  try {
    $response = Invoke-WebRequest `
      -Uri $uri `
      -Method Post `
      -ContentType "application/json" `
      -Body $body `
      -UseBasicParsing `
      -TimeoutSec 30

    return [pscustomobject]@{
      Response = $response
      StatusCode = [int]$response.StatusCode
      Body = if ([string]::IsNullOrWhiteSpace($response.Content)) { $null } else { $response.Content | ConvertFrom-Json }
    }
  }
  catch {
    $statusCode = $null
    $bodyText = ""

    if ($null -ne $_.Exception.Response) {
      $statusCode = [int]$_.Exception.Response.StatusCode
      try {
        $reader = New-Object IO.StreamReader($_.Exception.Response.GetResponseStream())
        $bodyText = $reader.ReadToEnd()
        $reader.Dispose()
      }
      catch {
        $bodyText = ""
      }
    }

    if ($null -ne $statusCode -and $statusCode -in $AllowedStatusCodes) {
      return [pscustomobject]@{
        Response = $_.Exception.Response
        StatusCode = $statusCode
        Body = if ([string]::IsNullOrWhiteSpace($bodyText)) { $null } else { $bodyText | ConvertFrom-Json }
      }
    }

    if (-not [string]::IsNullOrWhiteSpace($bodyText)) {
      throw "Auth request failed with status $statusCode at '$Path'. Response: $bodyText"
    }

    throw "Auth request failed with status $statusCode at '$Path'."
  }
}

function Invoke-RegisterOrLogin {
  $loginPayload = @{
    tenantId = $null
    emailOrUserName = $Email
    password = $Password
    rememberMe = $false
  }

  if (-not $LoginOnly) {
    $registerPayload = @{
      tenantName = $TenantName
      userName = $UserName
      email = $Email
      password = $Password
      firstName = "Search"
      lastName = "Dev"
      culture = "en-US"
    }

    Write-Log "Registering local dev auth user if needed: $Email"
    try {
      return Invoke-AuthRequest -Path "api/auth/register" -Payload $registerPayload
    }
    catch {
      $message = $_.Exception.ToString()
      if ($message -notmatch "409" -and
          $message -notmatch "duplicate" -and
          $message -notmatch "already exists" -and
          $message -notmatch "already registered") {
        throw
      }

      Write-Log "Local dev user already exists; falling back to login."
    }
  }

  Write-Log "Logging in local dev auth user: $Email"
  return Invoke-AuthRequest -Path "api/auth/login" -Payload $loginPayload
}

function Get-TokenPermissions {
  param([Parameter(Mandatory = $true)]$Payload)

  $permissions = @()
  foreach ($name in @("permission", "permissions", "scope", "scp")) {
    if ($null -eq $Payload.PSObject.Properties[$name]) {
      continue
    }

    $raw = $Payload.$name
    if ($raw -is [array]) {
      $permissions += $raw
      continue
    }

    $permissions += ([string]$raw).Split(",", [StringSplitOptions]::RemoveEmptyEntries -bor [StringSplitOptions]::TrimEntries)
  }

  return @($permissions | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)
}

Assert-LocalGatewayUrl -Url $GatewayBaseUrl
Require-Command -Name dotnet

if ([string]::IsNullOrWhiteSpace($Password)) {
  throw "Set NETMETRIC_DEV_SEED_PASSWORD or pass -Password before requesting a local auth token."
}

$repoRoot = Get-RepoRoot
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
  $OutputPath = Join-Path (Get-DevStateRoot) "search-auth-token.json"
}

Write-Log "Checking gateway readiness."
Wait-HttpOk -Url "$($GatewayBaseUrl.TrimEnd('/'))/health/ready" -TimeoutSeconds 120

$authResult = Invoke-RegisterOrLogin
$setCookies = Get-SetCookieValues -Response $authResult.Response
$accessToken = Get-CookieValue -SetCookieValues $setCookies -Name "netmetric-access"
if ([string]::IsNullOrWhiteSpace($accessToken)) {
  $accessToken = Get-CookieValue -SetCookieValues $setCookies -Name "__Secure-netmetric-access"
}

if ([string]::IsNullOrWhiteSpace($accessToken)) {
  throw "Auth response did not include a netmetric access cookie."
}

$payload = Decode-JwtPayload -Token $accessToken
$permissions = Get-TokenPermissions -Payload $payload
$hasWildcard = $permissions -contains "*"

if (-not $hasWildcard) {
  Write-Log "Warning: token does not include wildcard permission. CRM verification may fail unless specific CRM permissions are present."
}

$output = [pscustomobject]@{
  accessToken = $accessToken
  bearer = "Bearer $accessToken"
  tenantId = [string]$payload.tenant_id
  userId = [string]$payload.sub
  email = $Email
  gatewayBaseUrl = $GatewayBaseUrl.TrimEnd("/")
  expiresAtUtc = ([DateTimeOffset]::FromUnixTimeSeconds([int64]$payload.exp)).UtcDateTime.ToString("o")
  permissions = $permissions
}

$outputDirectory = Split-Path -Path $OutputPath -Parent
if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
  New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$output | ConvertTo-Json -Depth 8 | Set-Content -Path $OutputPath -Encoding UTF8

Write-Log "Wrote local dev search auth token details to: $OutputPath"
Write-Log "TenantId=$($output.tenantId) UserId=$($output.userId) WildcardPermission=$hasWildcard"

if ($PrintToken) {
  Write-Output $accessToken
}
