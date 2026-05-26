param(
  [string]$GatewayBaseUrl = "http://localhost:5030",
  [string]$TokenPath = "",
  [string]$AccountBearerToken = "",
  [string]$CrmBearerToken = "",
  [string]$DynamicCustomerQuery = "",
  [switch]$SkipAnonymous,
  [switch]$SkipAuthenticated
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "common.ps1")

$TrCustomersQuery = "m$([char]0x00FC)$([char]0x015F)teriler"
$TrContactsQuery = "ki$([char]0x015F)iler"

function Assert-LocalGatewayUrl {
  param([Parameter(Mandatory = $true)][string]$Url)

  $uri = [Uri]$Url
  $localHosts = @("localhost", "127.0.0.1", "::1")
  if ($uri.Scheme -ne "http" -or $uri.Host -notin $localHosts) {
    throw "Refusing to run search verification against a non-local gateway URL: $Url"
  }
}

function Resolve-TokenFromFile {
  param([string]$Path)

  if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path $Path)) {
    return $null
  }

  $payload = Get-Content -Path $Path -Raw | ConvertFrom-Json
  if ($null -ne $payload.PSObject.Properties["accessToken"] -and -not [string]::IsNullOrWhiteSpace($payload.accessToken)) {
    return [string]$payload.accessToken
  }

  return $null
}

function Normalize-BearerToken {
  param([string]$Token)

  if ([string]::IsNullOrWhiteSpace($Token)) {
    return ""
  }

  $trimmed = $Token.Trim()
  if ($trimmed.StartsWith("Bearer ", [StringComparison]::OrdinalIgnoreCase)) {
    return $trimmed.Substring(7).Trim()
  }

  return $trimmed
}

function Invoke-SearchQuery {
  param(
    [Parameter(Mandatory = $true)][string]$Query,
    [Parameter(Mandatory = $true)][string]$Locale,
    [string]$BearerToken = ""
  )

  $queryEscaped = [Uri]::EscapeDataString($Query)
  $localeEscaped = [Uri]::EscapeDataString($Locale)
  $uri = "$($GatewayBaseUrl.TrimEnd('/'))/api/v1/search?q=$queryEscaped&locale=$localeEscaped&pageSize=10"
  $headers = @{}
  $normalizedToken = Normalize-BearerToken -Token $BearerToken
  if (-not [string]::IsNullOrWhiteSpace($normalizedToken)) {
    $headers.Authorization = "Bearer $normalizedToken"
  }

  return Invoke-RestMethod -Uri $uri -Method Get -Headers $headers -TimeoutSec 30
}

function Assert-NoContentField {
  param(
    [Parameter(Mandatory = $true)]$Response,
    [Parameter(Mandatory = $true)][string]$Label
  )

  $items = @($Response.items)
  $withContent = @($items | Where-Object { $null -ne $_.PSObject.Properties["content"] })
  if ($withContent.Count -gt 0) {
    throw "$Label returned a content field, which must not be rendered or exposed in search results."
  }
}

function Assert-ExpectedHit {
  param(
    [Parameter(Mandatory = $true)][string]$Label,
    [Parameter(Mandatory = $true)][string]$Query,
    [Parameter(Mandatory = $true)][string]$Locale,
    [Parameter(Mandatory = $true)][string]$ExpectedId,
    [string]$BearerToken = ""
  )

  $response = Invoke-SearchQuery -Query $Query -Locale $Locale -BearerToken $BearerToken
  Assert-NoContentField -Response $response -Label $Label

  $hits = @($response.items | Where-Object { $_.id -eq $ExpectedId })
  if ($hits.Count -eq 0) {
    $ids = (@($response.items | ForEach-Object { $_.id }) -join ", ")
    throw "$Label failed: '$Query'/$Locale did not return '$ExpectedId'. Returned: $ids"
  }

  Write-Log "PASS $Label -> $ExpectedId"
}

function Assert-NoRestrictedAnonymousHits {
  param(
    [Parameter(Mandatory = $true)][string]$Label,
    [Parameter(Mandatory = $true)][string]$Query,
    [Parameter(Mandatory = $true)][string]$Locale
  )

  $response = Invoke-SearchQuery -Query $Query -Locale $Locale
  Assert-NoContentField -Response $response -Label $Label

  $restricted = @($response.items | Where-Object {
      $_.id -like "crm-*" -or
      $_.id -like "account-*" -or
      $_.source -in @("Crm", "Account", 3, 5)
    })

  if ($restricted.Count -gt 0) {
    $ids = ($restricted | ForEach-Object { $_.id }) -join ", "
    throw "$Label failed: anonymous search returned restricted docs: $ids"
  }

  Write-Log "PASS $Label"
}

function Assert-DynamicNeutralIfRequested {
  param([string]$BearerToken)

  if ([string]::IsNullOrWhiteSpace($DynamicCustomerQuery)) {
    Write-Log "SKIP dynamic neutral runtime check; pass -DynamicCustomerQuery when a customer fixture exists."
    return
  }

  if ([string]::IsNullOrWhiteSpace($BearerToken)) {
    throw "Dynamic neutral check requires -CrmBearerToken or a token file."
  }

  foreach ($locale in @("tr-TR", "en-US")) {
    $response = Invoke-SearchQuery -Query $DynamicCustomerQuery -Locale $locale -BearerToken $BearerToken
    Assert-NoContentField -Response $response -Label "Dynamic neutral $locale"

    $hits = @($response.items | Where-Object { $_.locale -eq "neutral" })
    if ($hits.Count -eq 0) {
      throw "Dynamic neutral failed: '$DynamicCustomerQuery' did not return a neutral doc for locale '$locale'."
    }
  }

  $anonymous = Invoke-SearchQuery -Query $DynamicCustomerQuery -Locale "tr-TR"
  Assert-NoContentField -Response $anonymous -Label "Dynamic neutral anonymous"
  $anonymousNeutralHits = @($anonymous.items | Where-Object { $_.locale -eq "neutral" })
  if ($anonymousNeutralHits.Count -gt 0) {
    throw "Dynamic neutral failed: anonymous search returned a neutral dynamic customer doc."
  }

  Write-Log "PASS dynamic neutral runtime checks."
}

Assert-LocalGatewayUrl -Url $GatewayBaseUrl

if ([string]::IsNullOrWhiteSpace($TokenPath)) {
  $TokenPath = Join-Path (Get-DevStateRoot) "search-auth-token.json"
}

$fileToken = Resolve-TokenFromFile -Path $TokenPath
if ([string]::IsNullOrWhiteSpace($AccountBearerToken) -and -not [string]::IsNullOrWhiteSpace($fileToken)) {
  $AccountBearerToken = $fileToken
}

if ([string]::IsNullOrWhiteSpace($CrmBearerToken) -and -not [string]::IsNullOrWhiteSpace($fileToken)) {
  $CrmBearerToken = $fileToken
}

Write-Log "Checking gateway readiness."
Wait-HttpOk -Url "$($GatewayBaseUrl.TrimEnd('/'))/health/ready" -TimeoutSeconds 120

if (-not $SkipAnonymous) {
  Assert-NoRestrictedAnonymousHits -Label "anonymous Turkish CRM customers hidden" -Query $TrCustomersQuery -Locale "tr-TR"
  Assert-NoRestrictedAnonymousHits -Label "anonymous Turkish account profile hidden" -Query "profil" -Locale "tr-TR"
  Assert-ExpectedHit -Label "anonymous English public pricing" -Query "pricing" -Locale "en-US" -ExpectedId "public-page-pricing-en-US"
}

if (-not $SkipAuthenticated) {
  if ([string]::IsNullOrWhiteSpace($AccountBearerToken)) {
    throw "Authenticated checks require -AccountBearerToken or a token file from scripts/dev/auth-token.ps1."
  }

  if ([string]::IsNullOrWhiteSpace($CrmBearerToken)) {
    throw "CRM checks require -CrmBearerToken or a token file from scripts/dev/auth-token.ps1."
  }

  Assert-ExpectedHit -Label "account profil tr-TR" -Query "profil" -Locale "tr-TR" -ExpectedId "account-page-profile-tr-TR" -BearerToken $AccountBearerToken
  Assert-ExpectedHit -Label "account oturumlar tr-TR" -Query "oturumlar" -Locale "tr-TR" -ExpectedId "account-page-sessions-tr-TR" -BearerToken $AccountBearerToken
  Assert-ExpectedHit -Label "account mfa tr-TR" -Query "mfa" -Locale "tr-TR" -ExpectedId "account-page-mfa-tr-TR" -BearerToken $AccountBearerToken
  Assert-ExpectedHit -Label "account profile en-US" -Query "profile" -Locale "en-US" -ExpectedId "account-page-profile-en-US" -BearerToken $AccountBearerToken

  Assert-ExpectedHit -Label "crm customers tr-TR" -Query $TrCustomersQuery -Locale "tr-TR" -ExpectedId "crm-module-customers-tr-TR" -BearerToken $CrmBearerToken
  Assert-ExpectedHit -Label "crm contacts tr-TR" -Query $TrContactsQuery -Locale "tr-TR" -ExpectedId "crm-module-contacts-tr-TR" -BearerToken $CrmBearerToken
  Assert-ExpectedHit -Label "crm tickets en-US" -Query "tickets" -Locale "en-US" -ExpectedId "crm-module-tickets-en-US" -BearerToken $CrmBearerToken

  Assert-DynamicNeutralIfRequested -BearerToken $CrmBearerToken
}

Write-Log "Phase 8I.2 authenticated localized search verification completed."
