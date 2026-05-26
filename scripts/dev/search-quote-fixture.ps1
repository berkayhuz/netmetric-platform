param(
  [string]$GatewayBaseUrl = "http://localhost:5030",
  [string]$TokenPath = "",
  [string]$BearerToken = "",
  [string]$QuoteNumber = "",
  [ValidateRange(15, 600)]
  [int]$TimeoutSeconds = 180,
  [ValidateRange(1, 30)]
  [int]$PollIntervalSeconds = 3,
  [switch]$SkipOutboxDiagnostics,
  [switch]$CreateViaOpportunityQuote,
  [switch]$DeleteAfter
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "common.ps1")

$RequiredPermissions = @(
  "quotes.manage",
  "quotes.read",
  "catalog.products.manage",
  "catalog.products.read"
)

if ($CreateViaOpportunityQuote) {
  $RequiredPermissions += @(
    "opportunities.manage",
    "opportunities.read",
    "opportunity.quotes.manage",
    "opportunity.quotes.read"
  )
}

function Assert-LocalGatewayUrl {
  param([Parameter(Mandatory = $true)][string]$Url)

  $uri = [Uri]$Url
  $localHosts = @("localhost", "127.0.0.1", "::1")
  if ($uri.Scheme -ne "http" -or $uri.Host -notin $localHosts) {
    throw "Refusing to create a CRM quote fixture through a non-local gateway URL: $Url"
  }
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

function Assert-TokenPermissions {
  param([Parameter(Mandatory = $true)][string]$Token)

  $payload = Decode-JwtPayload -Token $Token
  $permissions = Get-TokenPermissions -Payload $payload
  if ($permissions -contains "*") {
    Write-Log "Token has wildcard permission for local CRM fixture verification."
    return
  }

  $missing = @($RequiredPermissions | Where-Object { $_ -notin $permissions })
  if ($missing.Count -gt 0) {
    throw "Bearer token is missing required CRM permissions: $($missing -join ', '). Run scripts/dev/auth-token.ps1 or use a token with quote/catalog permissions."
  }

  Write-Log "Token has required quote and catalog permissions."
}

function Assert-TokenNotExpired {
  param([Parameter(Mandatory = $true)][string]$Token)

  $payload = Decode-JwtPayload -Token $Token
  if ($null -eq $payload.PSObject.Properties["exp"]) {
    return
  }

  $expiresAtUtc = [DateTimeOffset]::FromUnixTimeSeconds([int64]$payload.exp)
  if ($expiresAtUtc -le [DateTimeOffset]::UtcNow.AddSeconds(30)) {
    throw "Bearer token is expired (exp=$($expiresAtUtc.UtcDateTime.ToString('o'))). Run scripts/dev/auth-token.ps1 or rerun scripts/dev/search-full-verify.ps1 to mint a fresh token."
  }
}

function New-AuthHeaders {
  param([Parameter(Mandatory = $true)][string]$Token)

  return @{
    Authorization = "Bearer $Token"
  }
}

function Invoke-GatewayJson {
  param(
    [Parameter(Mandatory = $true)][string]$Path,
    [Parameter(Mandatory = $true)][string]$Method,
    [hashtable]$Headers = @{},
    [object]$Body = $null,
    [int[]]$AllowedStatusCodes = @(200)
  )

  $uri = "$($GatewayBaseUrl.TrimEnd('/'))/$($Path.TrimStart('/'))"
  $parameters = @{
    Uri = $uri
    Method = $Method
    Headers = $Headers
    UseBasicParsing = $true
    TimeoutSec = 30
  }

  if ($null -ne $Body) {
    $parameters.ContentType = "application/json"
    $parameters.Body = ($Body | ConvertTo-Json -Depth 10)
  }

  try {
    $response = Invoke-WebRequest @parameters
    $statusCode = [int]$response.StatusCode
    if ($statusCode -notin $AllowedStatusCodes) {
      throw "Gateway request failed with status $statusCode. $($response.Content)"
    }

    return [pscustomobject]@{
      StatusCode = $statusCode
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
        StatusCode = $statusCode
        Body = if ([string]::IsNullOrWhiteSpace($bodyText)) { $null } else { $bodyText | ConvertFrom-Json }
      }
    }

    if (-not [string]::IsNullOrWhiteSpace($bodyText)) {
      throw "Gateway request failed with status $statusCode. $bodyText"
    }

    throw
  }
}

function Invoke-SearchQuery {
  param(
    [Parameter(Mandatory = $true)][string]$Query,
    [Parameter(Mandatory = $true)][string]$Locale,
    [string]$Token = ""
  )

  $queryEscaped = [Uri]::EscapeDataString($Query)
  $localeEscaped = [Uri]::EscapeDataString($Locale)
  $headers = @{}
  if (-not [string]::IsNullOrWhiteSpace($Token)) {
    $headers = New-AuthHeaders -Token $Token
  }

  return (Invoke-GatewayJson `
      -Path "api/v1/search?q=$queryEscaped&locale=$localeEscaped&pageSize=10" `
      -Method "Get" `
      -Headers $headers `
      -AllowedStatusCodes @(200)).Body
}

function Assert-QuoteApiAuthorization {
  param([Parameter(Mandatory = $true)][string]$Token)

  try {
    (Invoke-GatewayJson `
      -Path "api/quotes?page=1&pageSize=1" `
      -Method "Get" `
      -Headers (New-AuthHeaders -Token $Token) `
      -AllowedStatusCodes @(200)) | Out-Null

    (Invoke-GatewayJson `
      -Path "api/catalog/products?page=1&pageSize=1" `
      -Method "Get" `
      -Headers (New-AuthHeaders -Token $Token) `
      -AllowedStatusCodes @(200)) | Out-Null
  } catch {
    $message = $_.Exception.Message
    if ($message -match "status 401") {
      throw "CRM quote/catalog API preflight returned 401. Token is missing/invalid/expired. Run scripts/dev/auth-token.ps1 with a valid dev password and retry."
    }

    if ($message -match "status 403") {
      throw "CRM quote/catalog API preflight returned 403. Token lacks Quote or Catalog permissions."
    }

    throw
  }
}

function Assert-NoContentField {
  param(
    [Parameter(Mandatory = $true)]$Response,
    [Parameter(Mandatory = $true)][string]$Label
  )

  $items = @($Response.items)
  $withContent = @($items | Where-Object { $null -ne $_.PSObject.Properties["content"] })
  if ($withContent.Count -gt 0) {
    throw "$Label returned a content field, which must not be exposed in search results."
  }
}

function Find-QuoteSearchHit {
  param(
    [Parameter(Mandatory = $true)]$Response,
    [Parameter(Mandatory = $true)][string]$ExpectedQuoteId,
    [Parameter(Mandatory = $true)][string]$ExpectedQuoteNumber
  )

  $quoteIdN = ([Guid]$ExpectedQuoteId).ToString("N")
  $quoteUrl = "/quotes/$ExpectedQuoteId"

  return @($Response.items | Where-Object {
      $_.locale -eq "neutral" -and
      $_.source -in @("Crm", 5) -and
      $_.type -eq "quote" -and
      ($_.title -eq $ExpectedQuoteNumber -or $_.url -eq $quoteUrl -or ([string]$_.id).Contains($quoteIdN))
    })
}

function Wait-QuoteSearchHit {
  param(
    [Parameter(Mandatory = $true)][string]$QuoteId,
    [Parameter(Mandatory = $true)][string]$ExpectedQuoteNumber,
    [Parameter(Mandatory = $true)][string]$Locale,
    [Parameter(Mandatory = $true)][string]$Token
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  $lastIds = ""
  $tenantId = ([Guid]$((Decode-JwtPayload -Token $Token).tenant_id)).ToString("N")
  $expectedDocumentId = "crm-quote-$tenantId-$(([Guid]$QuoteId).ToString('N'))"
  while ((Get-Date) -lt $deadline) {
    $response = Invoke-SearchQuery -Query $ExpectedQuoteNumber -Locale $Locale -Token $Token
    Assert-NoContentField -Response $response -Label "dynamic quote $Locale"

    $hits = @(Find-QuoteSearchHit -Response $response -ExpectedQuoteId $QuoteId -ExpectedQuoteNumber $ExpectedQuoteNumber)
    if ($hits.Count -gt 0) {
      Write-Log "PASS dynamic neutral quote $Locale -> $($hits[0].id)"
      return $hits[0]
    }

    $lastIds = (@($response.items | ForEach-Object { $_.id }) -join ", ")
    Start-Sleep -Seconds $PollIntervalSeconds
  }

  throw "Timed out waiting for dynamic quote '$ExpectedQuoteNumber' ($QuoteId) in locale '$Locale'. Expected document id hint: $expectedDocumentId. Last search ids: $lastIds. Diagnostics: confirm CRM API log contains 'QuoteManagement outbox processor started', confirm QuoteManagementOutboxMessages has a search.document.index.requested row, and confirm Search worker log shows search.index.crm consumption."
}

function Wait-QuoteSearchMiss {
  param(
    [Parameter(Mandatory = $true)][string]$QuoteId,
    [Parameter(Mandatory = $true)][string]$ExpectedQuoteNumber,
    [Parameter(Mandatory = $true)][string]$Locale,
    [Parameter(Mandatory = $true)][string]$Token
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  while ((Get-Date) -lt $deadline) {
    $response = Invoke-SearchQuery -Query $ExpectedQuoteNumber -Locale $Locale -Token $Token
    Assert-NoContentField -Response $response -Label "deleted dynamic quote $Locale"

    $hits = @(Find-QuoteSearchHit -Response $response -ExpectedQuoteId $QuoteId -ExpectedQuoteNumber $ExpectedQuoteNumber)
    if ($hits.Count -eq 0) {
      Write-Log "PASS deleted dynamic quote no longer appears for $Locale"
      return
    }

    Start-Sleep -Seconds $PollIntervalSeconds
  }

  throw "Timed out waiting for deleted dynamic quote '$ExpectedQuoteNumber' ($QuoteId) to disappear for locale '$Locale'."
}

function Assert-AnonymousCannotSeeQuote {
  param(
    [Parameter(Mandatory = $true)][string]$QuoteId,
    [Parameter(Mandatory = $true)][string]$ExpectedQuoteNumber
  )

  $response = Invoke-SearchQuery -Query $ExpectedQuoteNumber -Locale "tr-TR"
  Assert-NoContentField -Response $response -Label "dynamic quote anonymous"

  $hits = @(Find-QuoteSearchHit -Response $response -ExpectedQuoteId $QuoteId -ExpectedQuoteNumber $ExpectedQuoteNumber)
  if ($hits.Count -gt 0) {
    throw "Anonymous search returned the dynamic quote fixture '$ExpectedQuoteNumber'."
  }

  Write-Log "PASS anonymous search does not return dynamic quote fixture."
}

function Test-SearchWorkerProcess {
  $workerProcesses = @(Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue | Where-Object {
      ([string]$_.CommandLine) -match "NetMetric\.Search\.Worker"
    })

  return $workerProcesses.Count -gt 0
}

function New-UniqueQuoteNumber {
  if (-not [string]::IsNullOrWhiteSpace($QuoteNumber)) {
    return $QuoteNumber.Trim()
  }

  return "Q-SEARCH-$((Get-Date).ToUniversalTime().ToString('yyyyMMddHHmmss'))"
}

function Write-QuoteOutboxDiagnostics {
  param(
    [Parameter(Mandatory = $true)][string]$Stage,
    [switch]$AsWarning
  )

  $snapshot = Get-QuoteOutboxHealthSnapshot
  $message = "Quote outbox diagnostics [$Stage] -> available=$($snapshot.available) tableExists=$($snapshot.tableExists) pending=$($snapshot.pendingCount) retry=$($snapshot.retryCount) deadLetter=$($snapshot.deadLetterCount) processed=$($snapshot.processedCount) oldestPendingAgeSeconds=$($snapshot.oldestPendingAgeSeconds) recentFailures=$($snapshot.recentFailureCount)"
  if ($AsWarning) {
    Write-Log "Warning: $message"
  } else {
    Write-Log $message
  }

  if (-not [string]::IsNullOrWhiteSpace($snapshot.warning)) {
    if ($AsWarning) {
      Write-Log "Warning: Quote outbox diagnostics warning [$Stage]: $($snapshot.warning)"
    } else {
      Write-Log "Quote outbox diagnostics warning [$Stage]: $($snapshot.warning)"
    }
  }

  return $snapshot
}

function New-CatalogProductFixture {
  param(
    [Parameter(Mandatory = $true)][string]$Token,
    [Parameter(Mandatory = $true)][string]$Stamp
  )

  $payload = @{
    code = "QFIX-$Stamp"
    name = "Quote Fixture Product $Stamp"
    description = $null
    categoryId = $null
    unitPrice = 1
    currencyCode = "TRY"
  }

  Write-Log "Creating catalog product fixture for quote line validation."
  $created = Invoke-GatewayJson `
    -Path "api/catalog/products" `
    -Method "Post" `
    -Headers (New-AuthHeaders -Token $Token) `
    -Body $payload `
    -AllowedStatusCodes @(200, 201)

  if ($null -eq $created.Body -or $null -eq $created.Body.PSObject.Properties["id"]) {
    throw "Catalog product create response did not include an id."
  }

  return [string]$created.Body.id
}

function New-OpportunityFixture {
  param(
    [Parameter(Mandatory = $true)][string]$Token,
    [Parameter(Mandatory = $true)][string]$Stamp
  )

  $payload = @{
    opportunityCode = "OPP-Q-$Stamp"
    name = "Quote Cross Writer Opportunity $Stamp"
    description = $null
    estimatedAmount = 0
    expectedRevenue = $null
    probability = 0
    estimatedCloseDate = $null
    stage = 0
    status = 0
    priority = 1
    leadId = $null
    customerId = $null
    ownerUserId = $null
    notes = $null
  }

  Write-Log "Creating opportunity fixture for Opportunity quote cross-writer path."
  $created = Invoke-GatewayJson `
    -Path "api/opportunities" `
    -Method "Post" `
    -Headers (New-AuthHeaders -Token $Token) `
    -Body $payload `
    -AllowedStatusCodes @(200, 201)

  if ($null -eq $created.Body -or $null -eq $created.Body.PSObject.Properties["id"]) {
    throw "Opportunity create response did not include an id."
  }

  return [string]$created.Body.id
}

Assert-LocalGatewayUrl -Url $GatewayBaseUrl

if ([string]::IsNullOrWhiteSpace($TokenPath)) {
  $TokenPath = Join-Path (Get-DevStateRoot) "search-auth-token.json"
}

if ([string]::IsNullOrWhiteSpace($BearerToken)) {
  $BearerToken = Resolve-TokenFromFile -Path $TokenPath
}

$accessToken = Normalize-BearerToken -Token $BearerToken
if ([string]::IsNullOrWhiteSpace($accessToken)) {
  throw "A CRM bearer token is required. Run scripts/dev/auth-token.ps1 or pass -BearerToken."
}

Assert-TokenPermissions -Token $accessToken
Assert-TokenNotExpired -Token $accessToken

Write-Log "Checking gateway readiness."
Wait-HttpOk -Url "$($GatewayBaseUrl.TrimEnd('/'))/health/ready" -TimeoutSeconds 120
Assert-QuoteApiAuthorization -Token $accessToken

if (-not (Test-SearchWorkerProcess)) {
  Write-Log "Warning: Search worker process was not detected. Dynamic indexing requires the local Search worker and CRM outbox processor to be running."
}

$preCreateOutboxDiagnostics = $null
if (-not $SkipOutboxDiagnostics) {
  $preCreateOutboxDiagnostics = Write-QuoteOutboxDiagnostics -Stage "before-create"
} else {
  Write-Log "Skipping Quote outbox diagnostics because -SkipOutboxDiagnostics was provided."
}

$safeStamp = (Get-Date).ToUniversalTime().ToString("yyyyMMddHHmmss")
$productId = New-CatalogProductFixture -Token $accessToken -Stamp $safeStamp
$resolvedQuoteNumber = New-UniqueQuoteNumber
$creationPath = if ($CreateViaOpportunityQuote) { "opportunity" } else { "quote" }
$quotePayload = @{
  quoteNumber = $resolvedQuoteNumber
  proposalTitle = $null
  proposalSummary = $null
  proposalBody = $null
  quoteDate = (Get-Date).ToUniversalTime().ToString("o")
  validUntil = $null
  opportunityId = $null
  customerId = $null
  ownerUserId = $null
  currencyCode = "TRY"
  exchangeRate = 1
  termsAndConditions = $null
  proposalTemplateId = $null
  items = @(
    @{
      productId = $productId
      description = $null
      quantity = 1
      unitPrice = 1
      discountRate = 0
      taxRate = 0
    }
  )
}

if ($CreateViaOpportunityQuote) {
  $opportunityId = New-OpportunityFixture -Token $accessToken -Stamp $safeStamp
  $opportunityQuotePayload = @{
    quoteNumber = $resolvedQuoteNumber
    quoteDate = $quotePayload.quoteDate
    validUntil = $null
    termsAndConditions = $null
    ownerUserId = $null
    currencyCode = "TRY"
    exchangeRate = 1
    items = $quotePayload.items
  }

  Write-Log "Creating quote fixture through OpportunityManagement cross-writer path: $resolvedQuoteNumber"
  $created = Invoke-GatewayJson `
    -Path "api/opportunities/$opportunityId/quotes" `
    -Method "Post" `
    -Headers (New-AuthHeaders -Token $accessToken) `
    -Body $opportunityQuotePayload `
    -AllowedStatusCodes @(200, 201)
} else {
  Write-Log "Creating quote fixture through QuoteManagement API: $resolvedQuoteNumber"
  $created = Invoke-GatewayJson `
    -Path "api/quotes" `
    -Method "Post" `
    -Headers (New-AuthHeaders -Token $accessToken) `
    -Body $quotePayload `
    -AllowedStatusCodes @(200, 201)
}

if ($null -eq $created.Body -or $null -eq $created.Body.PSObject.Properties["id"]) {
  throw "CRM quote create response did not include an id."
}

$quoteId = [string]$created.Body.id
Write-Log "Created quote fixture. Id=$quoteId QuoteNumber='$resolvedQuoteNumber' Path=$creationPath"

$readBack = Invoke-GatewayJson `
  -Path "api/quotes/$quoteId" `
  -Method "Get" `
  -Headers (New-AuthHeaders -Token $accessToken) `
  -AllowedStatusCodes @(200)
if ($null -eq $readBack.Body -or [string]$readBack.Body.id -ne $quoteId) {
  throw "QuoteManagement read-back did not return the created quote. This would create a dead /quotes/$quoteId search URL."
}

Write-Log "Waiting for QuoteManagement outbox/Search worker indexing through authenticated search."

try {
  $trHit = Wait-QuoteSearchHit -QuoteId $quoteId -ExpectedQuoteNumber $resolvedQuoteNumber -Locale "tr-TR" -Token $accessToken
  $enHit = Wait-QuoteSearchHit -QuoteId $quoteId -ExpectedQuoteNumber $resolvedQuoteNumber -Locale "en-US" -Token $accessToken
} catch {
  if (-not $SkipOutboxDiagnostics) {
    $timeoutDiagnostics = Write-QuoteOutboxDiagnostics -Stage "timeout" -AsWarning
    if (-not $timeoutDiagnostics.tableExists) {
      Write-Log "Warning: QuoteManagementOutboxMessages table is missing. Run pnpm dev:up:nobuild to apply dev guard and restart CRM API with current code."
    } elseif ($timeoutDiagnostics.pendingCount -eq 0 -and $timeoutDiagnostics.processedCount -eq 0) {
      Write-Log "Warning: No quote outbox activity detected. This usually indicates stale CRM API binary or QuoteManagement outbox processor disabled."
    }
  }

  Write-Log "Warning: Diagnostic hint: check CRM API logs for 'QuoteManagement outbox processor started', verify QuoteManagement__Outbox__Enabled=true, and inspect Search Worker logs for search.index.crm consumption."
  throw
}

Assert-AnonymousCannotSeeQuote -QuoteId $quoteId -ExpectedQuoteNumber $resolvedQuoteNumber

if ($DeleteAfter) {
  Write-Log "Deleting quote fixture through QuoteManagement API because -DeleteAfter was provided."
  Invoke-GatewayJson `
    -Path "api/quotes/$quoteId" `
    -Method "Delete" `
    -Headers (New-AuthHeaders -Token $accessToken) `
    -AllowedStatusCodes @(200, 202, 204) | Out-Null

  Wait-QuoteSearchMiss -QuoteId $quoteId -ExpectedQuoteNumber $resolvedQuoteNumber -Locale "tr-TR" -Token $accessToken
  Wait-QuoteSearchMiss -QuoteId $quoteId -ExpectedQuoteNumber $resolvedQuoteNumber -Locale "en-US" -Token $accessToken
} else {
  Write-Log "Delete verification skipped; pass -DeleteAfter to soft-delete the fixture through the Quote API and verify search removal."
}

Write-Log "Dynamic neutral quote fixture verification completed."
Write-Output ([pscustomobject]@{
    quoteId = $quoteId
    quoteNumber = $resolvedQuoteNumber
    creationPath = $creationPath
    trLocaleDocumentId = [string]$trHit.id
    enLocaleDocumentId = [string]$enHit.id
    deleteVerified = [bool]$DeleteAfter
    outboxDiagnostics = $preCreateOutboxDiagnostics
  } | ConvertTo-Json -Depth 5)
