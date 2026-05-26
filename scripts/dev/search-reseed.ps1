param(
  [string]$MeilisearchEndpoint = "http://localhost:7700",
  [string]$MeilisearchApiKey = $env:NETMETRIC_DEV_MEILI_MASTER_KEY,
  [string]$IndexName = "searchdocuments",
  [string]$SearchApiReadyUrl = "http://localhost:5310/health/ready",
  [string]$SearchApiBaseUrl = "http://localhost:5310",
  [ValidateSet("Development", "Local", "local", "development")]
  [string]$Environment = "Development",
  [switch]$SkipSearchApiRestart,
  [string]$AccountBearerToken,
  [string]$CrmBearerToken,
  [string]$DynamicCustomerQuery,
  [switch]$AllowCustomLocalApiKey
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "common.ps1")
Add-Type -AssemblyName System.Net.Http

$DefaultDevMeilisearchApiKey = "NETMETRIC_DEV_MEILI_MASTER_KEY_2026_0001"
$TrCustomersTitle = "M$([char]0x00FC)$([char]0x015F)teriler"
$TrContactsTitle = "Ki$([char]0x015F)iler"
$TrCustomersQuery = "m$([char]0x00FC)$([char]0x015F)teriler"
$TrContactsQuery = "ki$([char]0x015F)iler"
$TrSecurityQuery = "g$([char]0x00FC)venlik"

function Resolve-MeilisearchApiKey {
  if (-not [string]::IsNullOrWhiteSpace($MeilisearchApiKey)) {
    return $MeilisearchApiKey
  }

  return $DefaultDevMeilisearchApiKey
}

function Assert-LocalDevSafety {
  param(
    [Parameter(Mandatory = $true)][string]$Endpoint,
    [Parameter(Mandatory = $true)][string]$ApiKey
  )

  if ($Environment -notin @("Development", "development", "Local", "local")) {
    throw "Refusing to run outside Development/local. Environment was '$Environment'."
  }

  $uri = [Uri]$Endpoint
  $localHosts = @("localhost", "127.0.0.1", "::1")
  if ($uri.Scheme -ne "http" -or $uri.Host -notin $localHosts) {
    throw "Refusing to reset a non-local Meilisearch endpoint: $Endpoint"
  }

  if ($IndexName -ne "searchdocuments") {
    throw "Refusing to reset index '$IndexName'. This script only resets the local searchdocuments index."
  }

  if ($ApiKey -ne $DefaultDevMeilisearchApiKey -and -not $AllowCustomLocalApiKey) {
    throw "Refusing to use a custom local Meilisearch API key without -AllowCustomLocalApiKey."
  }
}

function New-MeilisearchHeaders {
  param([Parameter(Mandatory = $true)][string]$ApiKey)

  return @{
    Authorization = "Bearer $ApiKey"
  }
}

function Get-ExceptionStatusCode {
  param([Parameter(Mandatory = $true)][object]$ErrorRecord)

  if ($null -eq $ErrorRecord.Exception -or
      $null -eq $ErrorRecord.Exception.PSObject.Properties["Response"] -or
      $null -eq $ErrorRecord.Exception.Response) {
    return $null
  }

  if ($null -eq $ErrorRecord.Exception.Response.PSObject.Properties["StatusCode"]) {
    return $null
  }

  return [int]$ErrorRecord.Exception.Response.StatusCode
}

function Invoke-MeilisearchRequest {
  param(
    [Parameter(Mandatory = $true)][string]$Path,
    [string]$Method = "Get",
    [object]$Body = $null,
    [int[]]$AllowedStatusCodes = @(200, 201, 202, 204)
  )

  $apiKey = Resolve-MeilisearchApiKey
  $uri = "$($MeilisearchEndpoint.TrimEnd('/'))/$($Path.TrimStart('/'))"
  $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::new($Method.ToUpperInvariant()), $uri)
  $request.Headers.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new("Bearer", $apiKey)

  try {
    if ($null -ne $Body) {
      $json = $Body | ConvertTo-Json -Depth 10
      $request.Content = [System.Net.Http.StringContent]::new($json, [Text.Encoding]::UTF8, "application/json")
    }

    $client = [System.Net.Http.HttpClient]::new()
    $client.Timeout = [TimeSpan]::FromSeconds(15)
    $response = $client.SendAsync($request).GetAwaiter().GetResult()
    $statusCode = [int]$response.StatusCode
    if ($statusCode -notin $AllowedStatusCodes) {
      $contentBytes = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
      $contentText = [Text.Encoding]::UTF8.GetString($contentBytes)
      throw "Meilisearch request failed with status $statusCode. $contentText"
    }

    if ($statusCode -ge 400) {
      return $null
    }

    $bytes = $response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
    if ($bytes.Length -eq 0) {
      return $null
    }

    return ([Text.Encoding]::UTF8.GetString($bytes) | ConvertFrom-Json)
  }
  catch {
    $statusCode = Get-ExceptionStatusCode -ErrorRecord $_

    if ($null -ne $statusCode -and $statusCode -in $AllowedStatusCodes) {
      return $null
    }

    throw
  }
}

function Wait-MeilisearchTask {
  param([Parameter(Mandatory = $true)][int]$TaskUid)

  $deadline = (Get-Date).AddSeconds(60)
  while ((Get-Date) -lt $deadline) {
    $task = Invoke-MeilisearchRequest -Path "tasks/$TaskUid"
    if ($task.status -eq "succeeded") {
      return
    }

    if ($task.status -in @("failed", "canceled")) {
      throw "Meilisearch task $TaskUid finished with status '$($task.status)'."
    }

    Start-Sleep -Milliseconds 500
  }

  throw "Timeout waiting for Meilisearch task $TaskUid."
}

function Remove-LocalSearchIndex {
  $encodedIndexName = [Uri]::EscapeDataString($IndexName)
  Write-Log "Deleting local Meilisearch index '$IndexName' if it exists."

  try {
    $result = Invoke-MeilisearchRequest -Path "indexes/$encodedIndexName" -Method "Delete" -AllowedStatusCodes @(200, 202, 204, 404)
    if ($null -ne $result -and $null -ne $result.taskUid) {
      Wait-MeilisearchTask -TaskUid ([int]$result.taskUid)
    }
  }
  catch {
    $statusCode = Get-ExceptionStatusCode -ErrorRecord $_

    if ($statusCode -eq 404) {
      Write-Log "Index '$IndexName' did not exist; continuing."
      return
    }

    throw
  }
}

function Start-SearchApiForStaticSeed {
  if ($SkipSearchApiRestart) {
    Write-Log "Skipping Search API restart; expecting an external Search API restart to seed the index."
    return
  }

  $repoRoot = Get-RepoRoot
  $projectPath = Join-Path $repoRoot "services\search\src\NetMetric.Search.API\NetMetric.Search.API.csproj"
  $messagesPath = Join-Path $repoRoot "packages\frontend\i18n\src\messages"

  if (-not (Test-Path $projectPath)) {
    throw "Search API project was not found: $projectPath"
  }

  Stop-DotNetRunForProject -ProjectPath $projectPath

  $envVars = @{
    ASPNETCORE_ENVIRONMENT = "Development"
    DOTNET_ENVIRONMENT = "Development"
    ASPNETCORE_URLS = "http://localhost:5310"
    LocalDevelopment__DisableHttpsRedirection = "true"
    Search__Provider = "Meilisearch"
    Search__IndexName = $IndexName
    Search__StaticIndexing__Enabled = "true"
    Search__StaticIndexing__SeedOnStartup = "true"
    Meilisearch__Endpoint = $MeilisearchEndpoint
    Meilisearch__ApiKey = (Resolve-MeilisearchApiKey)
    Authentication__Jwt__Issuer = "http://localhost:5297"
    Authentication__Jwt__Audience = "http://localhost:5030"
    Authentication__Jwt__Authority = "http://localhost:5297"
    Authentication__Jwt__MetadataAddress = "http://localhost:5297/.well-known/openid-configuration"
    NETMETRIC_SEARCH_I18N_MESSAGES_PATH = $messagesPath
  }

  Write-Log "Starting Search API in Development so StaticSearchIndexingHostedService can seed static documents."
  Start-DotNetProject `
    -Name "netmetric.search.api" `
    -ProjectPath $projectPath `
    -EnvironmentVariables $envVars
}

function Wait-SearchApiReady {
  Write-Log "Waiting for Search API readiness: $SearchApiReadyUrl"
  Wait-HttpOk -Url $SearchApiReadyUrl -TimeoutSeconds 120
}

function Wait-Document {
  param([Parameter(Mandatory = $true)][string]$DocumentId)

  $encodedIndexName = [Uri]::EscapeDataString($IndexName)
  $encodedDocumentId = [Uri]::EscapeDataString($DocumentId)
  $deadline = (Get-Date).AddSeconds(90)

  while ((Get-Date) -lt $deadline) {
    try {
      $document = Invoke-MeilisearchRequest -Path "indexes/$encodedIndexName/documents/$encodedDocumentId" -AllowedStatusCodes @(200, 404)
      if ($null -ne $document) {
        return $document
      }
    }
    catch {
      $statusCode = Get-ExceptionStatusCode -ErrorRecord $_

      if ($statusCode -ne 404) {
        throw
      }
    }

    Start-Sleep -Seconds 2
  }

  throw "Timed out waiting for document '$DocumentId'."
}

function Get-DocumentIfExists {
  param([Parameter(Mandatory = $true)][string]$DocumentId)

  $encodedIndexName = [Uri]::EscapeDataString($IndexName)
  $encodedDocumentId = [Uri]::EscapeDataString($DocumentId)

  try {
    return Invoke-MeilisearchRequest -Path "indexes/$encodedIndexName/documents/$encodedDocumentId" -AllowedStatusCodes @(200, 404)
  }
  catch {
    $statusCode = Get-ExceptionStatusCode -ErrorRecord $_

    if ($statusCode -eq 404) {
      return $null
    }

    throw
  }
}

function Assert-Document {
  param(
    [Parameter(Mandatory = $true)][object]$Document,
    [Parameter(Mandatory = $true)][string]$ExpectedId,
    [Parameter(Mandatory = $true)][string]$ExpectedTitle,
    [Parameter(Mandatory = $true)][string]$ExpectedLocale,
    [Parameter(Mandatory = $true)][string]$ExpectedUrl
  )

  if ($Document.id -ne $ExpectedId) {
    throw "Document '$ExpectedId' had unexpected id '$($Document.id)'."
  }

  if ($Document.title -ne $ExpectedTitle) {
    throw "Document '$ExpectedId' had unexpected title '$($Document.title)'. Expected '$ExpectedTitle'."
  }

  if ($Document.locale -ne $ExpectedLocale) {
    throw "Document '$ExpectedId' had unexpected locale '$($Document.locale)'. Expected '$ExpectedLocale'."
  }

  if ($Document.url -ne $ExpectedUrl) {
    throw "Document '$ExpectedId' had unexpected url '$($Document.url)'. Expected '$ExpectedUrl'."
  }

}

function Get-IndexDocumentCount {
  $encodedIndexName = [Uri]::EscapeDataString($IndexName)
  $stats = Invoke-MeilisearchRequest -Path "indexes/$encodedIndexName/stats"
  return [int]$stats.numberOfDocuments
}

function Assert-NoLegacyIds {
  $legacyIds = @("crm-module-customers", "account-page-profile")
  foreach ($legacyId in $legacyIds) {
    if ($null -ne (Get-DocumentIfExists -DocumentId $legacyId)) {
      throw "Legacy unsuffixed static document id still exists: $legacyId"
    }
  }
}

function Assert-NoLegacyUrlPrefixes {
  $encodedIndexName = [Uri]::EscapeDataString($IndexName)
  $page = Invoke-MeilisearchRequest -Path "indexes/$encodedIndexName/documents?limit=500"
  $results = @($page.results)
  $badDocuments = @($results | Where-Object {
      $_.url -like "/crm/*" -or $_.url -like "/account/*" -or $_.url -like "/tools/*"
    })

  if ($badDocuments.Count -gt 0) {
    $ids = ($badDocuments | Select-Object -First 10 | ForEach-Object { $_.id }) -join ", "
    throw "Found documents with legacy URL prefixes: $ids"
  }
}

function Invoke-SearchApiQuery {
  param(
    [Parameter(Mandatory = $true)][string]$Query,
    [Parameter(Mandatory = $true)][string]$Locale,
    [string]$BearerToken
  )

  $queryEscaped = [Uri]::EscapeDataString($Query)
  $localeEscaped = [Uri]::EscapeDataString($Locale)
  $uri = "$($SearchApiBaseUrl.TrimEnd('/'))/api/v1/search?q=$queryEscaped&locale=$localeEscaped&pageSize=10"
  $headers = @{}
  if (-not [string]::IsNullOrWhiteSpace($BearerToken)) {
    $headers.Authorization = "Bearer $BearerToken"
  }

  return Invoke-RestMethod -Uri $uri -Method Get -Headers $headers -TimeoutSec 20
}

function Invoke-MeilisearchQuery {
  param(
    [Parameter(Mandatory = $true)][string]$Query,
    [Parameter(Mandatory = $true)][string]$Locale
  )

  $encodedIndexName = [Uri]::EscapeDataString($IndexName)
  $body = @{
    q = $Query
    limit = 10
    filter = "(locale = `"$Locale`" OR locale = `"neutral`")"
  }

  return Invoke-MeilisearchRequest -Path "indexes/$encodedIndexName/search" -Method "Post" -Body $body
}

function Assert-MeilisearchSearchTerms {
  $checks = @(
    @{ Query = $TrCustomersQuery; Locale = "tr-TR"; ExpectedId = "crm-module-customers-tr-TR" },
    @{ Query = $TrContactsQuery; Locale = "tr-TR"; ExpectedId = "crm-module-contacts-tr-TR" },
    @{ Query = $TrSecurityQuery; Locale = "tr-TR"; ExpectedId = "account-page-security-tr-TR" },
    @{ Query = "oturumlar"; Locale = "tr-TR"; ExpectedId = "account-page-sessions-tr-TR" },
    @{ Query = "destek talepleri"; Locale = "tr-TR"; ExpectedId = "crm-module-tickets-tr-TR" },
    @{ Query = "customers"; Locale = "en-US"; ExpectedId = "crm-module-customers-en-US" },
    @{ Query = "contacts"; Locale = "en-US"; ExpectedId = "crm-module-contacts-en-US" },
    @{ Query = "security"; Locale = "en-US"; ExpectedId = "account-page-security-en-US" },
    @{ Query = "sessions"; Locale = "en-US"; ExpectedId = "account-page-sessions-en-US" },
    @{ Query = "tickets"; Locale = "en-US"; ExpectedId = "crm-module-tickets-en-US" }
  )

  foreach ($check in $checks) {
    $response = Invoke-MeilisearchQuery -Query $check.Query -Locale $check.Locale
    $hits = @($response.hits | Where-Object { $_.id -eq $check.ExpectedId })
    if ($hits.Count -eq 0) {
      throw "Meilisearch query '$($check.Query)'/$($check.Locale) did not return '$($check.ExpectedId)'."
    }
  }

  Write-Log "Turkish and English localized static search terms verified through Meilisearch."
}

function Assert-AnonymousSecurity {
  $crm = Invoke-SearchApiQuery -Query $TrCustomersQuery -Locale "tr-TR"
  $account = Invoke-SearchApiQuery -Query "profil" -Locale "tr-TR"
  $pricing = Invoke-SearchApiQuery -Query "pricing" -Locale "en-US"

  $restrictedHits = @($crm.items + $account.items | Where-Object {
      $_.source -in @("Crm", "Account", 3, 5) -or
      $_.id -like "crm-*" -or
      $_.id -like "account-*"
    })
  if ($restrictedHits.Count -gt 0) {
    throw "Anonymous search returned restricted CRM/Account docs."
  }

  $pricingHits = @($pricing.items | Where-Object { $_.id -eq "public-page-pricing-en-US" -or $_.title -eq "Pricing" })
  if ($pricingHits.Count -eq 0) {
    Write-Log "Warning: anonymous pricing search did not return Public Pricing."
  }

  Write-Log "Anonymous security verified: CRM/Account docs are not returned; public pricing check completed."
}

function Assert-AuthenticatedSearchIfTokenProvided {
  param(
    [string]$BearerToken,
    [Parameter(Mandatory = $true)][string]$Label,
    [Parameter(Mandatory = $true)][object[]]$Checks
  )

  if ([string]::IsNullOrWhiteSpace($BearerToken)) {
    Write-Log "$Label authenticated checks skipped; pass the matching bearer token parameter to enable them."
    return
  }

  foreach ($check in $Checks) {
    $response = Invoke-SearchApiQuery -Query $check.Query -Locale $check.Locale -BearerToken $BearerToken
    $hits = @($response.items | Where-Object { $_.id -eq $check.ExpectedId })
    if ($hits.Count -eq 0) {
      throw "$Label authenticated search '$($check.Query)'/$($check.Locale) did not return '$($check.ExpectedId)'."
    }
  }

  Write-Log "$Label authenticated search checks passed."
}

function Assert-DynamicNeutralIfAvailable {
  if ([string]::IsNullOrWhiteSpace($DynamicCustomerQuery)) {
    Write-Log "Dynamic neutral runtime check skipped; pass -DynamicCustomerQuery when a customer fixture exists."
    return
  }

  if ([string]::IsNullOrWhiteSpace($CrmBearerToken)) {
    Write-Log "Dynamic neutral runtime check skipped; pass -CrmBearerToken with -DynamicCustomerQuery."
    return
  }

  foreach ($locale in @("tr-TR", "en-US")) {
    $response = Invoke-SearchApiQuery -Query $DynamicCustomerQuery -Locale $locale -BearerToken $CrmBearerToken
    $hits = @($response.items | Where-Object { $_.locale -eq "neutral" })
    if ($hits.Count -eq 0) {
      throw "Dynamic neutral customer search '$DynamicCustomerQuery' did not return a neutral doc for locale '$locale'."
    }
  }

  Write-Log "Dynamic neutral runtime checks passed for '$DynamicCustomerQuery'."
}

Require-Command -Name docker
Require-Command -Name dotnet

$apiKey = Resolve-MeilisearchApiKey
Assert-LocalDevSafety -Endpoint $MeilisearchEndpoint -ApiKey $apiKey

$repoRoot = Get-RepoRoot
Set-Location $repoRoot

Write-Log "Phase 8I.1 local search reseed starting. This resets only '$IndexName' on '$MeilisearchEndpoint'."
Wait-DockerReady
Invoke-Compose -ComposeArgs @("up", "-d", "netmetric-meilisearch")
Wait-HttpOk -Url "$($MeilisearchEndpoint.TrimEnd('/'))/health" -TimeoutSeconds 120

Remove-LocalSearchIndex
Start-SearchApiForStaticSeed
Wait-SearchApiReady

$expectedDocuments = @(
  @{ Id = "crm-module-customers-en-US"; Title = "Customers"; Locale = "en-US"; Url = "/customers" },
  @{ Id = "crm-module-customers-tr-TR"; Title = $TrCustomersTitle; Locale = "tr-TR"; Url = "/customers" },
  @{ Id = "crm-module-contacts-en-US"; Title = "Contacts"; Locale = "en-US"; Url = "/contacts" },
  @{ Id = "crm-module-contacts-tr-TR"; Title = $TrContactsTitle; Locale = "tr-TR"; Url = "/contacts" },
  @{ Id = "account-page-profile-en-US"; Title = "Profile"; Locale = "en-US"; Url = "/profile" },
  @{ Id = "account-page-profile-tr-TR"; Title = "Profil"; Locale = "tr-TR"; Url = "/profile" },
  @{ Id = "account-page-mfa-en-US"; Title = "MFA"; Locale = "en-US"; Url = "/security/mfa" },
  @{ Id = "account-page-mfa-tr-TR"; Title = "MFA"; Locale = "tr-TR"; Url = "/security/mfa" }
)

foreach ($expected in $expectedDocuments) {
  $document = Wait-Document -DocumentId $expected.Id
  Assert-Document `
    -Document $document `
    -ExpectedId $expected.Id `
    -ExpectedTitle $expected.Title `
    -ExpectedLocale $expected.Locale `
    -ExpectedUrl $expected.Url
}

Assert-NoLegacyIds
Assert-NoLegacyUrlPrefixes
Assert-MeilisearchSearchTerms

$documentCount = Get-IndexDocumentCount
Write-Log "Meilisearch '$IndexName' document count after reseed: $documentCount"
Write-Log "Localized document IDs verified: $($expectedDocuments.Id -join ', ')"

Assert-AnonymousSecurity

Assert-AuthenticatedSearchIfTokenProvided `
  -BearerToken $AccountBearerToken `
  -Label "Account" `
  -Checks @(
    @{ Query = "profil"; Locale = "tr-TR"; ExpectedId = "account-page-profile-tr-TR" },
    @{ Query = "oturumlar"; Locale = "tr-TR"; ExpectedId = "account-page-sessions-tr-TR" },
    @{ Query = "mfa"; Locale = "tr-TR"; ExpectedId = "account-page-mfa-tr-TR" },
    @{ Query = "profile"; Locale = "en-US"; ExpectedId = "account-page-profile-en-US" }
  )

Assert-AuthenticatedSearchIfTokenProvided `
  -BearerToken $CrmBearerToken `
  -Label "CRM" `
  -Checks @(
    @{ Query = $TrCustomersQuery; Locale = "tr-TR"; ExpectedId = "crm-module-customers-tr-TR" },
    @{ Query = $TrContactsQuery; Locale = "tr-TR"; ExpectedId = "crm-module-contacts-tr-TR" },
    @{ Query = "tickets"; Locale = "en-US"; ExpectedId = "crm-module-tickets-en-US" }
  )

Assert-DynamicNeutralIfAvailable

Write-Log "Phase 8I.1 local search reseed completed."
