param(
  [string]$GatewayBaseUrl = "http://localhost:5030",
  [string]$TokenPath = "",
  [string]$BearerToken = "",
  [string]$LeadName = "",
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
  "leads.manage",
  "leads.read"
)

function Assert-LocalGatewayUrl {
  param([Parameter(Mandatory = $true)][string]$Url)

  $uri = [Uri]$Url
  $localHosts = @("localhost", "127.0.0.1", "::1")
  if ($uri.Scheme -ne "http" -or $uri.Host -notin $localHosts) {
    throw "Refusing to create a CRM lead fixture through a non-local gateway URL: $Url"
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
    throw "Bearer token is missing required CRM permissions: $($missing -join ', '). Run scripts/dev/auth-token.ps1 or use a token with lead manage/read permissions."
  }

  Write-Log "Token has required lead manage/read permissions."
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

function Find-LeadSearchHit {
  param(
    [Parameter(Mandatory = $true)]$Response,
    [Parameter(Mandatory = $true)][string]$ExpectedLeadId,
    [Parameter(Mandatory = $true)][string]$ExpectedLeadName
  )

  $leadIdN = ([Guid]$ExpectedLeadId).ToString("N")
  $leadUrl = "/leads/$ExpectedLeadId"

  return @($Response.items | Where-Object {
      $_.locale -eq "neutral" -and
      $_.source -in @("Crm", 5) -and
      $_.type -eq "lead" -and
      ($_.title -eq $ExpectedLeadName -or $_.url -eq $leadUrl -or ([string]$_.id).Contains($leadIdN))
    })
}

function Wait-LeadSearchHit {
  param(
    [Parameter(Mandatory = $true)][string]$LeadId,
    [Parameter(Mandatory = $true)][string]$LeadName,
    [Parameter(Mandatory = $true)][string]$Locale,
    [Parameter(Mandatory = $true)][string]$Token
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  $lastIds = ""
  $expectedDocumentId = "crm-lead-$(([Guid]$((Decode-JwtPayload -Token $Token).tenant_id)).ToString('N'))-$(([Guid]$LeadId).ToString('N'))"
  while ((Get-Date) -lt $deadline) {
    $response = Invoke-SearchQuery -Query $LeadName -Locale $Locale -Token $Token
    Assert-NoContentField -Response $response -Label "dynamic lead $Locale"

    $hits = @(Find-LeadSearchHit -Response $response -ExpectedLeadId $LeadId -ExpectedLeadName $LeadName)
    if ($hits.Count -gt 0) {
      Write-Log "PASS dynamic neutral lead $Locale -> $($hits[0].id)"
      return $hits[0]
    }

    $lastIds = (@($response.items | ForEach-Object { $_.id }) -join ", ")
    Start-Sleep -Seconds $PollIntervalSeconds
  }

  throw "Timed out waiting for dynamic lead '$LeadName' ($LeadId) in locale '$Locale'. Expected document id hint: $expectedDocumentId. Last search ids: $lastIds"
}

function Wait-LeadSearchMiss {
  param(
    [Parameter(Mandatory = $true)][string]$LeadId,
    [Parameter(Mandatory = $true)][string]$LeadName,
    [Parameter(Mandatory = $true)][string]$Locale,
    [Parameter(Mandatory = $true)][string]$Token
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  while ((Get-Date) -lt $deadline) {
    $response = Invoke-SearchQuery -Query $LeadName -Locale $Locale -Token $Token
    Assert-NoContentField -Response $response -Label "deleted dynamic lead $Locale"

    $hits = @(Find-LeadSearchHit -Response $response -ExpectedLeadId $LeadId -ExpectedLeadName $LeadName)
    if ($hits.Count -eq 0) {
      Write-Log "PASS deleted dynamic lead no longer appears for $Locale"
      return
    }

    Start-Sleep -Seconds $PollIntervalSeconds
  }

  throw "Timed out waiting for deleted dynamic lead '$LeadName' ($LeadId) to disappear for locale '$Locale'."
}

function Assert-AnonymousCannotSeeLead {
  param(
    [Parameter(Mandatory = $true)][string]$LeadId,
    [Parameter(Mandatory = $true)][string]$LeadName
  )

  $response = Invoke-SearchQuery -Query $LeadName -Locale "tr-TR"
  Assert-NoContentField -Response $response -Label "dynamic lead anonymous"

  $hits = @(Find-LeadSearchHit -Response $response -ExpectedLeadId $LeadId -ExpectedLeadName $LeadName)
  if ($hits.Count -gt 0) {
    throw "Anonymous search returned the dynamic lead fixture '$LeadName'."
  }

  Write-Log "PASS anonymous search does not return dynamic lead fixture."
}

function Test-SearchWorkerProcess {
  $workerProcesses = @(Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue | Where-Object {
      ([string]$_.CommandLine) -match "NetMetric\.Search\.Worker"
    })

  return $workerProcesses.Count -gt 0
}

function New-UniqueLeadName {
  if (-not [string]::IsNullOrWhiteSpace($LeadName)) {
    return $LeadName.Trim()
  }

  return "Search Neutral Lead $((Get-Date).ToUniversalTime().ToString('yyyyMMddHHmmss'))"
}

function Write-LeadOutboxDiagnostics {
  param(
    [Parameter(Mandatory = $true)][string]$Stage,
    [switch]$AsWarning
  )

  $snapshot = Get-LeadOutboxHealthSnapshot
  $message = "Lead outbox diagnostics [$Stage] -> available=$($snapshot.available) tableExists=$($snapshot.tableExists) pending=$($snapshot.pendingCount) retry=$($snapshot.retryCount) deadLetter=$($snapshot.deadLetterCount) processed=$($snapshot.processedCount) oldestPendingAgeSeconds=$($snapshot.oldestPendingAgeSeconds) recentFailures=$($snapshot.recentFailureCount)"
  if ($AsWarning) {
    Write-Log "Warning: $message"
  } else {
    Write-Log $message
  }

  if (-not [string]::IsNullOrWhiteSpace($snapshot.warning)) {
    if ($AsWarning) {
      Write-Log "Warning: Lead outbox diagnostics warning [$Stage]: $($snapshot.warning)"
    } else {
      Write-Log "Lead outbox diagnostics warning [$Stage]: $($snapshot.warning)"
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
  $preCreateOutboxDiagnostics = Write-LeadOutboxDiagnostics -Stage "before-create"
} else {
  Write-Log "Skipping Lead outbox diagnostics because -SkipOutboxDiagnostics was provided."
}

$fixtureName = New-UniqueLeadName
$payload = @{
  fullName = $fixtureName
  source = 0
  status = 0
  priority = 1
}

Write-Log "Creating local CRM lead fixture through gateway: $fixtureName"
$created = Invoke-GatewayJson `
  -Path "api/leads" `
  -Method "Post" `
  -Headers (New-AuthHeaders -Token $accessToken) `
  -Body $payload `
  -AllowedStatusCodes @(200, 201)

if ($null -eq $created.Body -or $null -eq $created.Body.PSObject.Properties["id"]) {
  throw "CRM lead create response did not include an id."
}

$leadId = [string]$created.Body.id
$leadName = if ($null -ne $created.Body.PSObject.Properties["fullName"] -and -not [string]::IsNullOrWhiteSpace($created.Body.fullName)) {
  [string]$created.Body.fullName
} else {
  $fixtureName
}

Write-Log "Created lead fixture. Id=$leadId Name='$leadName'"
Write-Log "Waiting for LeadManagement outbox/Search worker indexing through authenticated search."

try {
  $trHit = Wait-LeadSearchHit -LeadId $leadId -LeadName $leadName -Locale "tr-TR" -Token $accessToken
  $enHit = Wait-LeadSearchHit -LeadId $leadId -LeadName $leadName -Locale "en-US" -Token $accessToken
} catch {
  if (-not $SkipOutboxDiagnostics) {
    $timeoutDiagnostics = Write-LeadOutboxDiagnostics -Stage "timeout" -AsWarning
    if (-not $timeoutDiagnostics.tableExists) {
      Write-Log "Warning: LeadManagementOutboxMessages table is missing. Restart local stack with pnpm dev:up:nobuild and ensure CRM API runs current code."
    } elseif ($timeoutDiagnostics.pendingCount -eq 0 -and $timeoutDiagnostics.processedCount -eq 0) {
      Write-Log "Warning: No lead outbox activity detected. This usually indicates stale CRM API binary or LeadManagement outbox processor disabled."
    }
  }

  Write-Log "Warning: Diagnostic hint: check CRM API logs for 'LeadManagement outbox processor started', verify LeadManagement__Outbox__Enabled=true, and inspect Search Worker logs for search.index.crm consumption."
  throw
}

Assert-AnonymousCannotSeeLead -LeadId $leadId -LeadName $leadName

if ($DeleteAfter) {
  Write-Log "Deleting lead fixture through CRM API because -DeleteAfter was provided."
  Invoke-GatewayJson `
    -Path "api/leads/$leadId" `
    -Method "Delete" `
    -Headers (New-AuthHeaders -Token $accessToken) `
    -AllowedStatusCodes @(200, 202, 204) | Out-Null

  Wait-LeadSearchMiss -LeadId $leadId -LeadName $leadName -Locale "tr-TR" -Token $accessToken
  Wait-LeadSearchMiss -LeadId $leadId -LeadName $leadName -Locale "en-US" -Token $accessToken
} else {
  Write-Log "Delete verification skipped; pass -DeleteAfter to soft-delete the fixture through the CRM API and verify search removal."
}

Write-Log "Dynamic neutral lead fixture verification completed."
Write-Output ([pscustomobject]@{
    leadId = $leadId
    leadName = $leadName
    trLocaleDocumentId = [string]$trHit.id
    enLocaleDocumentId = [string]$enHit.id
    deleteVerified = [bool]$DeleteAfter
    outboxDiagnostics = $preCreateOutboxDiagnostics
  } | ConvertTo-Json -Depth 5)
