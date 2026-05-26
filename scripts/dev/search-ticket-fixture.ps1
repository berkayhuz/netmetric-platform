param(
  [string]$GatewayBaseUrl = "http://localhost:5030",
  [string]$TokenPath = "",
  [string]$BearerToken = "",
  [string]$TicketSubject = "",
  [ValidateRange(15, 600)]
  [int]$TimeoutSeconds = 180,
  [ValidateRange(1, 30)]
  [int]$PollIntervalSeconds = 3,
  [switch]$SkipOutboxDiagnostics,
  [switch]$DeleteAfter
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "common.ps1")

$RequiredPermissions = @(
  "tickets.manage",
  "tickets.read"
)

function Assert-LocalGatewayUrl {
  param([Parameter(Mandatory = $true)][string]$Url)

  $uri = [Uri]$Url
  $localHosts = @("localhost", "127.0.0.1", "::1")
  if ($uri.Scheme -ne "http" -or $uri.Host -notin $localHosts) {
    throw "Refusing to create a CRM ticket fixture through a non-local gateway URL: $Url"
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
    throw "Bearer token is missing required CRM permissions: $($missing -join ', '). Run scripts/dev/auth-token.ps1 or use a token with ticket manage/read permissions."
  }

  Write-Log "Token has required ticket manage/read permissions."
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

function Find-TicketSearchHit {
  param(
    [Parameter(Mandatory = $true)]$Response,
    [Parameter(Mandatory = $true)][string]$ExpectedTicketId,
    [Parameter(Mandatory = $true)][string]$ExpectedTicketSubject
  )

  $ticketIdN = ([Guid]$ExpectedTicketId).ToString("N")
  $ticketUrl = "/tickets/$ExpectedTicketId"

  return @($Response.items | Where-Object {
      $_.locale -eq "neutral" -and
      $_.source -in @("Crm", 5) -and
      $_.type -eq "ticket" -and
      ($_.title -eq $ExpectedTicketSubject -or $_.url -eq $ticketUrl -or ([string]$_.id).Contains($ticketIdN))
    })
}

function Wait-TicketSearchHit {
  param(
    [Parameter(Mandatory = $true)][string]$TicketId,
    [Parameter(Mandatory = $true)][string]$TicketSubject,
    [Parameter(Mandatory = $true)][string]$Locale,
    [Parameter(Mandatory = $true)][string]$Token
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  $lastIds = ""
  $expectedDocumentId = "crm-ticket-$(([Guid]$((Decode-JwtPayload -Token $Token).tenant_id)).ToString('N'))-$(([Guid]$TicketId).ToString('N'))"
  while ((Get-Date) -lt $deadline) {
    $response = Invoke-SearchQuery -Query $TicketSubject -Locale $Locale -Token $Token
    Assert-NoContentField -Response $response -Label "dynamic ticket $Locale"

    $hits = @(Find-TicketSearchHit -Response $response -ExpectedTicketId $TicketId -ExpectedTicketSubject $TicketSubject)
    if ($hits.Count -gt 0) {
      Write-Log "PASS dynamic neutral ticket $Locale -> $($hits[0].id)"
      return $hits[0]
    }

    $lastIds = (@($response.items | ForEach-Object { $_.id }) -join ", ")
    Start-Sleep -Seconds $PollIntervalSeconds
  }

  throw "Timed out waiting for dynamic ticket '$TicketSubject' ($TicketId) in locale '$Locale'. Expected document id hint: $expectedDocumentId. Last search ids: $lastIds. Diagnostics: confirm CRM API log contains 'TicketManagement outbox processor started', confirm TicketManagementOutboxMessages has a search.document.index.requested row, and confirm Search worker log shows search.index.crm consumption."
}

function Wait-TicketSearchMiss {
  param(
    [Parameter(Mandatory = $true)][string]$TicketId,
    [Parameter(Mandatory = $true)][string]$TicketSubject,
    [Parameter(Mandatory = $true)][string]$Locale,
    [Parameter(Mandatory = $true)][string]$Token
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  while ((Get-Date) -lt $deadline) {
    $response = Invoke-SearchQuery -Query $TicketSubject -Locale $Locale -Token $Token
    Assert-NoContentField -Response $response -Label "deleted dynamic ticket $Locale"

    $hits = @(Find-TicketSearchHit -Response $response -ExpectedTicketId $TicketId -ExpectedTicketSubject $TicketSubject)
    if ($hits.Count -eq 0) {
      Write-Log "PASS deleted dynamic ticket no longer appears for $Locale"
      return
    }

    Start-Sleep -Seconds $PollIntervalSeconds
  }

  throw "Timed out waiting for deleted dynamic ticket '$TicketSubject' ($TicketId) to disappear for locale '$Locale'."
}

function Assert-AnonymousCannotSeeTicket {
  param(
    [Parameter(Mandatory = $true)][string]$TicketId,
    [Parameter(Mandatory = $true)][string]$TicketSubject
  )

  $response = Invoke-SearchQuery -Query $TicketSubject -Locale "tr-TR"
  Assert-NoContentField -Response $response -Label "dynamic ticket anonymous"

  $hits = @(Find-TicketSearchHit -Response $response -ExpectedTicketId $TicketId -ExpectedTicketSubject $TicketSubject)
  if ($hits.Count -gt 0) {
    throw "Anonymous search returned the dynamic ticket fixture '$TicketSubject'."
  }

  Write-Log "PASS anonymous search does not return dynamic ticket fixture."
}

function Test-SearchWorkerProcess {
  $workerProcesses = @(Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue | Where-Object {
      ([string]$_.CommandLine) -match "NetMetric\.Search\.Worker"
    })

  return $workerProcesses.Count -gt 0
}

function New-UniqueTicketSubject {
  if (-not [string]::IsNullOrWhiteSpace($TicketSubject)) {
    return $TicketSubject.Trim()
  }

  return "Search Neutral Ticket $((Get-Date).ToUniversalTime().ToString('yyyyMMddHHmmss'))"
}

function Write-TicketOutboxDiagnostics {
  param(
    [Parameter(Mandatory = $true)][string]$Stage,
    [switch]$AsWarning
  )

  $snapshot = Get-TicketOutboxHealthSnapshot
  $message = "Ticket outbox diagnostics [$Stage] -> available=$($snapshot.available) tableExists=$($snapshot.tableExists) pending=$($snapshot.pendingCount) retry=$($snapshot.retryCount) deadLetter=$($snapshot.deadLetterCount) processed=$($snapshot.processedCount) oldestPendingAgeSeconds=$($snapshot.oldestPendingAgeSeconds) recentFailures=$($snapshot.recentFailureCount)"
  if ($AsWarning) {
    Write-Log "Warning: $message"
  } else {
    Write-Log $message
  }

  if (-not [string]::IsNullOrWhiteSpace($snapshot.warning)) {
    if ($AsWarning) {
      Write-Log "Warning: Ticket outbox diagnostics warning [$Stage]: $($snapshot.warning)"
    } else {
      Write-Log "Ticket outbox diagnostics warning [$Stage]: $($snapshot.warning)"
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

Write-Log "Checking gateway readiness."
Wait-HttpOk -Url "$($GatewayBaseUrl.TrimEnd('/'))/health/ready" -TimeoutSeconds 120

if (-not (Test-SearchWorkerProcess)) {
  Write-Log "Warning: Search worker process was not detected. Dynamic indexing requires the local Search worker and CRM outbox processor to be running."
}

$preCreateOutboxDiagnostics = $null
if (-not $SkipOutboxDiagnostics) {
  $preCreateOutboxDiagnostics = Write-TicketOutboxDiagnostics -Stage "before-create"
} else {
  Write-Log "Skipping Ticket outbox diagnostics because -SkipOutboxDiagnostics was provided."
}

$fixtureSubject = New-UniqueTicketSubject
$payload = @{
  subject = $fixtureSubject
  description = "Local dev ticket dynamic neutral fixture."
}

Write-Log "Creating local CRM ticket fixture through gateway: $fixtureSubject"
$created = Invoke-GatewayJson `
  -Path "api/tickets" `
  -Method "Post" `
  -Headers (New-AuthHeaders -Token $accessToken) `
  -Body $payload `
  -AllowedStatusCodes @(200, 201)

if ($null -eq $created.Body -or $null -eq $created.Body.PSObject.Properties["id"]) {
  throw "CRM ticket create response did not include an id."
}

$ticketId = [string]$created.Body.id
$resolvedTicketSubject = if ($null -ne $created.Body.PSObject.Properties["subject"] -and -not [string]::IsNullOrWhiteSpace($created.Body.subject)) {
  [string]$created.Body.subject
} else {
  $fixtureSubject
}

Write-Log "Created ticket fixture. Id=$ticketId Subject='$resolvedTicketSubject'"
Write-Log "Waiting for TicketManagement outbox/Search worker indexing through authenticated search."

try {
  $trHit = Wait-TicketSearchHit -TicketId $ticketId -TicketSubject $resolvedTicketSubject -Locale "tr-TR" -Token $accessToken
  $enHit = Wait-TicketSearchHit -TicketId $ticketId -TicketSubject $resolvedTicketSubject -Locale "en-US" -Token $accessToken
} catch {
  if (-not $SkipOutboxDiagnostics) {
    $timeoutDiagnostics = Write-TicketOutboxDiagnostics -Stage "timeout" -AsWarning
    if (-not $timeoutDiagnostics.tableExists) {
      Write-Log "Warning: TicketManagementOutboxMessages table is missing. Run pnpm dev:up:nobuild to apply dev guard and restart CRM API with current code."
    }
  }

  Write-Log "Warning: Diagnostic hint: ensure CRM API and Search Worker are running, then retry search-full-verify after pnpm dev:up:nobuild."
  throw
}

Assert-AnonymousCannotSeeTicket -TicketId $ticketId -TicketSubject $resolvedTicketSubject

if ($DeleteAfter) {
  Write-Log "Deleting ticket fixture through CRM API because -DeleteAfter was provided."
  Invoke-GatewayJson `
    -Path "api/tickets/$ticketId" `
    -Method "Delete" `
    -Headers (New-AuthHeaders -Token $accessToken) `
    -AllowedStatusCodes @(200, 202, 204) | Out-Null

  Wait-TicketSearchMiss -TicketId $ticketId -TicketSubject $resolvedTicketSubject -Locale "tr-TR" -Token $accessToken
  Wait-TicketSearchMiss -TicketId $ticketId -TicketSubject $resolvedTicketSubject -Locale "en-US" -Token $accessToken
} else {
  Write-Log "Delete verification skipped; pass -DeleteAfter to soft-delete the fixture through the CRM API and verify search removal."
}

Write-Log "Dynamic neutral ticket fixture verification completed."
Write-Output ([pscustomobject]@{
    ticketId = $ticketId
    ticketSubject = $resolvedTicketSubject
    trLocaleDocumentId = [string]$trHit.id
    enLocaleDocumentId = [string]$enHit.id
    deleteVerified = [bool]$DeleteAfter
    outboxDiagnostics = $preCreateOutboxDiagnostics
  } | ConvertTo-Json -Depth 5)
