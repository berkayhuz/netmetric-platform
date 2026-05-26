param(
  [string]$GatewayBaseUrl = "http://localhost:5030",
  [string]$TokenPath = "",
  [string]$BearerToken = "",
  [string]$DealName = "",
  [ValidateRange(15, 600)]
  [int]$TimeoutSeconds = 180,
  [ValidateRange(1, 30)]
  [int]$PollIntervalSeconds = 3,
  [switch]$SkipOutboxDiagnostics,
  [switch]$DeleteAfter,
  [switch]$CreateViaOpportunityWon
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "common.ps1")

$RequiredPermissions = @("deals.manage", "deals.read")
if ($CreateViaOpportunityWon) {
  $RequiredPermissions += @("opportunities.manage", "opportunities.read")
}

function Assert-LocalGatewayUrl {
  param([Parameter(Mandatory = $true)][string]$Url)

  $uri = [Uri]$Url
  $localHosts = @("localhost", "127.0.0.1", "::1")
  if ($uri.Scheme -ne "http" -or $uri.Host -notin $localHosts) {
    throw "Refusing to create a CRM deal fixture through a non-local gateway URL: $Url"
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
    throw "Bearer token is missing required CRM permissions: $($missing -join ', '). Run scripts/dev/auth-token.ps1 or use a token with deal manage/read permissions."
  }

  Write-Log "Token has required deal manage/read permissions."
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
    $parameters.Body = ($Body | ConvertTo-Json -Depth 8)
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

function Assert-DealApiAuthorization {
  param([Parameter(Mandatory = $true)][string]$Token)

  try {
    (Invoke-GatewayJson `
      -Path "api/deals?page=1&pageSize=1" `
      -Method "Get" `
      -Headers (New-AuthHeaders -Token $Token) `
      -AllowedStatusCodes @(200)) | Out-Null
  } catch {
    $message = $_.Exception.Message
    if ($message -match "status 401") {
      throw "CRM deal API preflight returned 401. Token is missing/invalid/expired for Deal endpoints. Run scripts/dev/auth-token.ps1 with a valid dev password and retry."
    }

    if ($message -match "status 403") {
      throw "CRM deal API preflight returned 403. Token lacks Deal permissions (deals.read/deals.manage). Use a token with Deal permissions and retry."
    }

    throw
  }
}

function Assert-OpportunityApiAuthorization {
  param([Parameter(Mandatory = $true)][string]$Token)

  try {
    (Invoke-GatewayJson `
      -Path "api/opportunities?page=1&pageSize=1" `
      -Method "Get" `
      -Headers (New-AuthHeaders -Token $Token) `
      -AllowedStatusCodes @(200)) | Out-Null
  } catch {
    $message = $_.Exception.Message
    if ($message -match "status 401") {
      throw "CRM opportunity API preflight returned 401. Token is missing/invalid/expired for Opportunity endpoints."
    }

    if ($message -match "status 403") {
      throw "CRM opportunity API preflight returned 403. Token lacks Opportunity permissions (opportunities.read/opportunities.manage)."
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

function Find-DealSearchHit {
  param(
    [Parameter(Mandatory = $true)]$Response,
    [Parameter(Mandatory = $true)][string]$ExpectedDealId,
    [Parameter(Mandatory = $true)][string]$ExpectedDealName
  )

  $dealIdN = ([Guid]$ExpectedDealId).ToString("N")
  $dealUrl = "/deals/$ExpectedDealId"

  return @($Response.items | Where-Object {
      $_.locale -eq "neutral" -and
      $_.source -in @("Crm", 5) -and
      $_.type -eq "deal" -and
      ($_.title -eq $ExpectedDealName -or $_.url -eq $dealUrl -or ([string]$_.id).Contains($dealIdN))
    })
}

function Wait-DealSearchHit {
  param(
    [Parameter(Mandatory = $true)][string]$DealId,
    [Parameter(Mandatory = $true)][string]$DealName,
    [Parameter(Mandatory = $true)][string]$Locale,
    [Parameter(Mandatory = $true)][string]$Token
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  $lastIds = ""
  $expectedDocumentId = "crm-deal-$(([Guid]$((Decode-JwtPayload -Token $Token).tenant_id)).ToString('N'))-$(([Guid]$DealId).ToString('N'))"
  while ((Get-Date) -lt $deadline) {
    $response = Invoke-SearchQuery -Query $DealName -Locale $Locale -Token $Token
    Assert-NoContentField -Response $response -Label "dynamic deal $Locale"

    $hits = @(Find-DealSearchHit -Response $response -ExpectedDealId $DealId -ExpectedDealName $DealName)
    if ($hits.Count -gt 0) {
      Write-Log "PASS dynamic neutral deal $Locale -> $($hits[0].id)"
      return $hits[0]
    }

    $lastIds = (@($response.items | ForEach-Object { $_.id }) -join ", ")
    Start-Sleep -Seconds $PollIntervalSeconds
  }

  throw "Timed out waiting for dynamic deal '$DealName' ($DealId) in locale '$Locale'. Expected document id hint: $expectedDocumentId. Last search ids: $lastIds. Diagnostics: confirm CRM API log contains 'DealManagement outbox processor started', confirm DealManagementOutboxMessages has a search.document.index.requested row, and confirm Search worker log shows search.index.crm consumption."
}

function Wait-DealSearchMiss {
  param(
    [Parameter(Mandatory = $true)][string]$DealId,
    [Parameter(Mandatory = $true)][string]$DealName,
    [Parameter(Mandatory = $true)][string]$Locale,
    [Parameter(Mandatory = $true)][string]$Token
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  while ((Get-Date) -lt $deadline) {
    $response = Invoke-SearchQuery -Query $DealName -Locale $Locale -Token $Token
    Assert-NoContentField -Response $response -Label "deleted dynamic deal $Locale"

    $hits = @(Find-DealSearchHit -Response $response -ExpectedDealId $DealId -ExpectedDealName $DealName)
    if ($hits.Count -eq 0) {
      Write-Log "PASS deleted dynamic deal no longer appears for $Locale"
      return
    }

    Start-Sleep -Seconds $PollIntervalSeconds
  }

  throw "Timed out waiting for deleted dynamic deal '$DealName' ($DealId) to disappear for locale '$Locale'."
}

function Assert-AnonymousCannotSeeDeal {
  param(
    [Parameter(Mandatory = $true)][string]$DealId,
    [Parameter(Mandatory = $true)][string]$DealName
  )

  $response = Invoke-SearchQuery -Query $DealName -Locale "tr-TR"
  Assert-NoContentField -Response $response -Label "dynamic deal anonymous"

  $hits = @(Find-DealSearchHit -Response $response -ExpectedDealId $DealId -ExpectedDealName $DealName)
  if ($hits.Count -gt 0) {
    throw "Anonymous search returned the dynamic deal fixture '$DealName'."
  }

  Write-Log "PASS anonymous search does not return dynamic deal fixture."
}

function Test-SearchWorkerProcess {
  $workerProcesses = @(Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue | Where-Object {
      ([string]$_.CommandLine) -match "NetMetric\.Search\.Worker"
    })

  return $workerProcesses.Count -gt 0
}

function New-UniqueDealName {
  if (-not [string]::IsNullOrWhiteSpace($DealName)) {
    return $DealName.Trim()
  }

  return "Search Neutral Deal $((Get-Date).ToUniversalTime().ToString('yyyyMMddHHmmss'))"
}

function Write-DealOutboxDiagnostics {
  param(
    [Parameter(Mandatory = $true)][string]$Stage,
    [switch]$AsWarning
  )

  $snapshot = Get-DealOutboxHealthSnapshot
  $message = "Deal outbox diagnostics [$Stage] -> available=$($snapshot.available) tableExists=$($snapshot.tableExists) pending=$($snapshot.pendingCount) retry=$($snapshot.retryCount) deadLetter=$($snapshot.deadLetterCount) processed=$($snapshot.processedCount) oldestPendingAgeSeconds=$($snapshot.oldestPendingAgeSeconds) recentFailures=$($snapshot.recentFailureCount)"
  if ($AsWarning) {
    Write-Log "Warning: $message"
  } else {
    Write-Log $message
  }

  if (-not [string]::IsNullOrWhiteSpace($snapshot.warning)) {
    if ($AsWarning) {
      Write-Log "Warning: Deal outbox diagnostics warning [$Stage]: $($snapshot.warning)"
    } else {
      Write-Log "Deal outbox diagnostics warning [$Stage]: $($snapshot.warning)"
    }
  }

  return $snapshot
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
Assert-DealApiAuthorization -Token $accessToken
if ($CreateViaOpportunityWon) {
  Assert-OpportunityApiAuthorization -Token $accessToken
}

if (-not (Test-SearchWorkerProcess)) {
  Write-Log "Warning: Search worker process was not detected. Dynamic indexing requires the local Search worker and CRM outbox processor to be running."
}

$preCreateOutboxDiagnostics = $null
if (-not $SkipOutboxDiagnostics) {
  $preCreateOutboxDiagnostics = Write-DealOutboxDiagnostics -Stage "before-create"
} else {
  Write-Log "Skipping Deal outbox diagnostics because -SkipOutboxDiagnostics was provided."
}

$fixtureName = New-UniqueDealName
$safeStamp = (Get-Date).ToUniversalTime().ToString("yyyyMMddHHmmss")
$dealId = ""
$resolvedDealName = $fixtureName
$deletePath = ""
$creationPath = if ($CreateViaOpportunityWon) { "opportunity-won" } else { "deal-management" }

if ($CreateViaOpportunityWon) {
  $opportunityName = "Search Opp Won $safeStamp"
  $createOpportunityPayload = @{
    opportunityCode = "OPP-$safeStamp"
    name = $opportunityName
    description = $null
    estimatedAmount = 12000.50
    expectedRevenue = 13000.50
    probability = 50
    estimatedCloseDate = (Get-Date).ToUniversalTime().AddDays(7).ToString("o")
    stage = 0
    status = 0
    priority = 1
    leadId = $null
    customerId = $null
    ownerUserId = $null
    notes = $null
  }

  Write-Log "Creating opportunity fixture through gateway for MarkOpportunityWon path: $opportunityName"
  $opportunityCreated = Invoke-GatewayJson `
    -Path "api/opportunities" `
    -Method "Post" `
    -Headers (New-AuthHeaders -Token $accessToken) `
    -Body $createOpportunityPayload `
    -AllowedStatusCodes @(200, 201)

  if ($null -eq $opportunityCreated.Body -or $null -eq $opportunityCreated.Body.PSObject.Properties["id"]) {
    throw "CRM opportunity create response did not include an id."
  }

  $opportunityId = [string]$opportunityCreated.Body.id
  $markWonPayload = @{
    dealName = $fixtureName
    closedDate = (Get-Date).ToUniversalTime().ToString("o")
  }

  Write-Log "Marking opportunity won to create cross-writer deal fixture. OpportunityId=$opportunityId"
  $wonResult = Invoke-GatewayJson `
    -Path "api/opportunities/$opportunityId/won" `
    -Method "Post" `
    -Headers (New-AuthHeaders -Token $accessToken) `
    -Body $markWonPayload `
    -AllowedStatusCodes @(200)

  if ($null -eq $wonResult.Body -or $null -eq $wonResult.Body.PSObject.Properties["dealId"] -or [string]::IsNullOrWhiteSpace([string]$wonResult.Body.dealId)) {
    throw "MarkOpportunityWon response did not include dealId."
  }

  $dealId = [string]$wonResult.Body.dealId
  $resolvedDealName = $fixtureName
  $deletePath = "api/deals/$dealId"
} else {
  $payload = @{
    dealCode = "DEAL-$safeStamp"
    name = $fixtureName
    totalAmount = 12345.67
    closedDate = (Get-Date).ToUniversalTime().ToString("o")
    opportunityId = $null
    companyId = $null
    ownerUserId = $null
    notes = "Local dev deal dynamic neutral fixture."
  }

  Write-Log "Creating local CRM deal fixture through gateway: $fixtureName"
  $created = Invoke-GatewayJson `
    -Path "api/deals" `
    -Method "Post" `
    -Headers (New-AuthHeaders -Token $accessToken) `
    -Body $payload `
    -AllowedStatusCodes @(200, 201)

  if ($null -eq $created.Body -or $null -eq $created.Body.PSObject.Properties["id"]) {
    throw "CRM deal create response did not include an id."
  }

  $dealId = [string]$created.Body.id
  $resolvedDealName = if ($null -ne $created.Body.PSObject.Properties["name"] -and -not [string]::IsNullOrWhiteSpace($created.Body.name)) {
    [string]$created.Body.name
  } else {
    $fixtureName
  }
  $deletePath = "api/deals/$dealId"
}

Write-Log "Created deal fixture. Id=$dealId Name='$resolvedDealName'"
Write-Log "Waiting for DealManagement outbox/Search worker indexing through authenticated search."

try {
  $trHit = Wait-DealSearchHit -DealId $dealId -DealName $resolvedDealName -Locale "tr-TR" -Token $accessToken
  $enHit = Wait-DealSearchHit -DealId $dealId -DealName $resolvedDealName -Locale "en-US" -Token $accessToken
} catch {
  if (-not $SkipOutboxDiagnostics) {
    $timeoutDiagnostics = Write-DealOutboxDiagnostics -Stage "timeout" -AsWarning
    if (-not $timeoutDiagnostics.tableExists) {
      Write-Log "Warning: DealManagementOutboxMessages table is missing. Run pnpm dev:up:nobuild to apply dev guard and restart CRM API with current code."
    }
  }

  Write-Log "Warning: Diagnostic hint: ensure CRM API and Search Worker are running, then retry search-full-verify after pnpm dev:up:nobuild."
  throw
}

Assert-AnonymousCannotSeeDeal -DealId $dealId -DealName $resolvedDealName

if ($DeleteAfter) {
  Write-Log "Deleting deal fixture through CRM API because -DeleteAfter was provided."
  Invoke-GatewayJson `
    -Path $deletePath `
    -Method "Delete" `
    -Headers (New-AuthHeaders -Token $accessToken) `
    -AllowedStatusCodes @(200, 202, 204) | Out-Null

  Wait-DealSearchMiss -DealId $dealId -DealName $resolvedDealName -Locale "tr-TR" -Token $accessToken
  Wait-DealSearchMiss -DealId $dealId -DealName $resolvedDealName -Locale "en-US" -Token $accessToken
} else {
  Write-Log "Delete verification skipped; pass -DeleteAfter to soft-delete the fixture through the CRM API and verify search removal."
}

Write-Log "Dynamic neutral deal fixture verification completed."
Write-Output ([pscustomobject]@{
    dealId = $dealId
    dealName = $resolvedDealName
    creationPath = $creationPath
    trLocaleDocumentId = [string]$trHit.id
    enLocaleDocumentId = [string]$enHit.id
    deleteVerified = [bool]$DeleteAfter
    outboxDiagnostics = $preCreateOutboxDiagnostics
  } | ConvertTo-Json -Depth 5)
