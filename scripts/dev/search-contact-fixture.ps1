param(
  [string]$GatewayBaseUrl = "http://localhost:5030",
  [string]$TokenPath = "",
  [string]$BearerToken = "",
  [string]$ContactName = "",
  [string]$CompanyId = "",
  [string]$CustomerId = "",
  [switch]$CreateLinkedCompany,
  [ValidateRange(15, 600)]
  [int]$TimeoutSeconds = 180,
  [ValidateRange(1, 30)]
  [int]$PollIntervalSeconds = 3,
  [switch]$DeleteAfter
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "common.ps1")

$ContactRequiredPermissions = @(
  "crm.customer-management.contacts.manage",
  "crm.customer-management.contacts.read"
)

$CompanyCreateRequiredPermissions = @(
  "crm.customer-management.companies.manage"
)

function Assert-LocalGatewayUrl {
  param([Parameter(Mandatory = $true)][string]$Url)

  $uri = [Uri]$Url
  $localHosts = @("localhost", "127.0.0.1", "::1")
  if ($uri.Scheme -ne "http" -or $uri.Host -notin $localHosts) {
    throw "Refusing to create a CRM contact fixture through a non-local gateway URL: $Url"
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

  $missing = @($ContactRequiredPermissions | Where-Object { $_ -notin $permissions })
  if ($missing.Count -gt 0) {
    throw "Bearer token is missing required CRM permissions: $($missing -join ', '). Run scripts/dev/auth-token.ps1 or use a token with CRM contact manage/read permissions."
  }

  Write-Log "Token has required CRM contact manage/read permissions."
}

function Assert-CompanyCreatePermissionIfNeeded {
  param(
    [Parameter(Mandatory = $true)][string]$Token,
    [Parameter(Mandatory = $true)][bool]$WillCreateLinkedCompany
  )

  if (-not $WillCreateLinkedCompany) {
    return
  }

  $payload = Decode-JwtPayload -Token $Token
  $permissions = Get-TokenPermissions -Payload $payload
  if ($permissions -contains "*") {
    return
  }

  $missing = @($CompanyCreateRequiredPermissions | Where-Object { $_ -notin $permissions })
  if ($missing.Count -gt 0) {
    throw "Creating a linked company requires additional CRM permissions: $($missing -join ', '). Pass -CompanyId or -CustomerId, or use a token with company manage permission."
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

function Find-ContactSearchHit {
  param(
    [Parameter(Mandatory = $true)]$Response,
    [Parameter(Mandatory = $true)][string]$ExpectedContactId,
    [Parameter(Mandatory = $true)][string]$ExpectedContactName
  )

  $contactIdN = ([Guid]$ExpectedContactId).ToString("N")
  $contactUrl = "/contacts/$ExpectedContactId"

  return @($Response.items | Where-Object {
      $_.locale -eq "neutral" -and
      $_.source -in @("Crm", 5) -and
      $_.type -eq "contact" -and
      ($_.title -eq $ExpectedContactName -or $_.url -eq $contactUrl -or ([string]$_.id).Contains($contactIdN))
    })
}

function Wait-ContactSearchHit {
  param(
    [Parameter(Mandatory = $true)][string]$ContactId,
    [Parameter(Mandatory = $true)][string]$ContactName,
    [Parameter(Mandatory = $true)][string]$Locale,
    [Parameter(Mandatory = $true)][string]$Token
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  $lastIds = ""
  while ((Get-Date) -lt $deadline) {
    $response = Invoke-SearchQuery -Query $ContactName -Locale $Locale -Token $Token
    Assert-NoContentField -Response $response -Label "dynamic contact $Locale"

    $hits = @(Find-ContactSearchHit -Response $response -ExpectedContactId $ContactId -ExpectedContactName $ContactName)
    if ($hits.Count -gt 0) {
      Write-Log "PASS dynamic neutral contact $Locale -> $($hits[0].id)"
      return $hits[0]
    }

    $lastIds = (@($response.items | ForEach-Object { $_.id }) -join ", ")
    Start-Sleep -Seconds $PollIntervalSeconds
  }

  throw "Timed out waiting for dynamic contact '$ContactName' ($ContactId) in locale '$Locale'. Last search ids: $lastIds"
}

function Wait-ContactSearchMiss {
  param(
    [Parameter(Mandatory = $true)][string]$ContactId,
    [Parameter(Mandatory = $true)][string]$ContactName,
    [Parameter(Mandatory = $true)][string]$Locale,
    [Parameter(Mandatory = $true)][string]$Token
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  while ((Get-Date) -lt $deadline) {
    $response = Invoke-SearchQuery -Query $ContactName -Locale $Locale -Token $Token
    Assert-NoContentField -Response $response -Label "deleted dynamic contact $Locale"

    $hits = @(Find-ContactSearchHit -Response $response -ExpectedContactId $ContactId -ExpectedContactName $ContactName)
    if ($hits.Count -eq 0) {
      Write-Log "PASS deleted dynamic contact no longer appears for $Locale"
      return
    }

    Start-Sleep -Seconds $PollIntervalSeconds
  }

  throw "Timed out waiting for deleted dynamic contact '$ContactName' ($ContactId) to disappear for locale '$Locale'."
}

function Assert-AnonymousCannotSeeContact {
  param(
    [Parameter(Mandatory = $true)][string]$ContactId,
    [Parameter(Mandatory = $true)][string]$ContactName
  )

  $response = Invoke-SearchQuery -Query $ContactName -Locale "tr-TR"
  Assert-NoContentField -Response $response -Label "dynamic contact anonymous"

  $hits = @(Find-ContactSearchHit -Response $response -ExpectedContactId $ContactId -ExpectedContactName $ContactName)
  if ($hits.Count -gt 0) {
    throw "Anonymous search returned the dynamic contact fixture '$ContactName'."
  }

  Write-Log "PASS anonymous search does not return dynamic contact fixture."
}

function Test-SearchWorkerProcess {
  $workerProcesses = @(Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue | Where-Object {
      ([string]$_.CommandLine) -match "NetMetric\.Search\.Worker"
    })

  return $workerProcesses.Count -gt 0
}

function New-UniqueContactName {
  if (-not [string]::IsNullOrWhiteSpace($ContactName)) {
    return $ContactName.Trim()
  }

  return "Search Neutral Contact $((Get-Date).ToUniversalTime().ToString('yyyyMMddHHmmss'))"
}

function New-UniqueCompanyName {
  return "Search Linked Company $((Get-Date).ToUniversalTime().ToString('yyyyMMddHHmmss'))"
}

function Resolve-LinkedEntityInput {
  param(
    [string]$ProvidedCompanyId,
    [string]$ProvidedCustomerId
  )

  $company = if ([string]::IsNullOrWhiteSpace($ProvidedCompanyId)) { "" } else { $ProvidedCompanyId.Trim() }
  $customer = if ([string]::IsNullOrWhiteSpace($ProvidedCustomerId)) { "" } else { $ProvidedCustomerId.Trim() }

  if (-not [string]::IsNullOrWhiteSpace($company)) {
    $company = ([Guid]$company).ToString()
  }

  if (-not [string]::IsNullOrWhiteSpace($customer)) {
    $customer = ([Guid]$customer).ToString()
  }

  if (-not [string]::IsNullOrWhiteSpace($company) -and -not [string]::IsNullOrWhiteSpace($customer)) {
    throw "Pass either -CompanyId or -CustomerId for contact fixture linking, not both."
  }

  return [pscustomobject]@{
    CompanyId = $company
    CustomerId = $customer
  }
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
$linked = Resolve-LinkedEntityInput -ProvidedCompanyId $CompanyId -ProvidedCustomerId $CustomerId
$scriptCreatedLinkedCompany = $false
$linkedCompanyId = $linked.CompanyId
$linkedCustomerId = $linked.CustomerId
$createdLinkedCompanyId = ""
$createdLinkedCompanyName = ""

$shouldCreateLinkedCompany = [string]::IsNullOrWhiteSpace($linkedCompanyId) -and [string]::IsNullOrWhiteSpace($linkedCustomerId)
if ($shouldCreateLinkedCompany -and -not $CreateLinkedCompany) {
  Write-Log "No -CompanyId or -CustomerId provided; creating a temporary linked company by default."
}
Assert-CompanyCreatePermissionIfNeeded -Token $accessToken -WillCreateLinkedCompany $shouldCreateLinkedCompany

Write-Log "Checking gateway readiness."
Wait-HttpOk -Url "$($GatewayBaseUrl.TrimEnd('/'))/health/ready" -TimeoutSeconds 120

if (-not (Test-SearchWorkerProcess)) {
  Write-Log "Warning: Search worker process was not detected. Dynamic indexing requires the local Search worker and CRM outbox processor to be running."
}

$fixtureName = New-UniqueContactName
$nameParts = $fixtureName.Split(" ", 2, [StringSplitOptions]::RemoveEmptyEntries)
$firstName = if ($nameParts.Count -gt 1) { $nameParts[0] } else { "Search" }
$lastName = if ($nameParts.Count -gt 1) { $nameParts[1] } else { $fixtureName }

$contactId = ""
$resolvedContactName = ""

if ($shouldCreateLinkedCompany) {
  $createdLinkedCompanyName = New-UniqueCompanyName
  $companyPayload = @{
    name = $createdLinkedCompanyName
    isActive = $true
  }

  Write-Log "Creating temporary linked CRM company through gateway: $createdLinkedCompanyName"
  $createdCompany = Invoke-GatewayJson `
    -Path "api/companies" `
    -Method "Post" `
    -Headers (New-AuthHeaders -Token $accessToken) `
    -Body $companyPayload `
    -AllowedStatusCodes @(200, 201)

  if ($null -eq $createdCompany.Body -or $null -eq $createdCompany.Body.PSObject.Properties["id"]) {
    throw "Temporary linked company create response did not include an id."
  }

  $createdLinkedCompanyId = [string]$createdCompany.Body.id
  $linkedCompanyId = $createdLinkedCompanyId
  $scriptCreatedLinkedCompany = $true
  Write-Log "Created temporary linked company. Id=$createdLinkedCompanyId Name='$createdLinkedCompanyName'"
}

$payload = @{
  firstName = $firstName
  lastName = $lastName
  isActive = $true
}

if (-not [string]::IsNullOrWhiteSpace($linkedCompanyId)) {
  $payload.companyId = $linkedCompanyId
}

if (-not [string]::IsNullOrWhiteSpace($linkedCustomerId)) {
  $payload.customerId = $linkedCustomerId
}

Write-Log "Creating local CRM contact fixture through gateway: $fixtureName"
$created = Invoke-GatewayJson `
  -Path "api/v1/contacts" `
  -Method "Post" `
  -Headers (New-AuthHeaders -Token $accessToken) `
  -Body $payload `
  -AllowedStatusCodes @(200, 201)

try {
  if ($null -eq $created.Body -or $null -eq $created.Body.PSObject.Properties["id"]) {
    throw "CRM contact create response did not include an id."
  }

  $contactId = [string]$created.Body.id
  $resolvedContactName = if ($null -ne $created.Body.PSObject.Properties["fullName"] -and -not [string]::IsNullOrWhiteSpace($created.Body.fullName)) {
    [string]$created.Body.fullName
  } else {
    $fixtureName
  }
}
catch {
  if ($scriptCreatedLinkedCompany -and -not [string]::IsNullOrWhiteSpace($createdLinkedCompanyId)) {
    Write-Log "Contact creation failed; deleting temporary linked company fixture."
    Invoke-GatewayJson `
      -Path "api/companies/$createdLinkedCompanyId" `
      -Method "Delete" `
      -Headers (New-AuthHeaders -Token $accessToken) `
      -AllowedStatusCodes @(200, 202, 204) | Out-Null
  }

  throw
}

Write-Log "Created contact fixture. Id=$contactId Name='$resolvedContactName'"
Write-Log "Waiting for CRM outbox/Search worker indexing through authenticated search."

$trHit = Wait-ContactSearchHit -ContactId $contactId -ContactName $resolvedContactName -Locale "tr-TR" -Token $accessToken
$enHit = Wait-ContactSearchHit -ContactId $contactId -ContactName $resolvedContactName -Locale "en-US" -Token $accessToken

Assert-AnonymousCannotSeeContact -ContactId $contactId -ContactName $resolvedContactName

if ($DeleteAfter) {
  Write-Log "Deleting contact fixture through CRM API because -DeleteAfter was provided."
  Invoke-GatewayJson `
    -Path "api/v1/contacts/$contactId" `
    -Method "Delete" `
    -Headers (New-AuthHeaders -Token $accessToken) `
    -AllowedStatusCodes @(200, 202, 204) | Out-Null

  Wait-ContactSearchMiss -ContactId $contactId -ContactName $resolvedContactName -Locale "tr-TR" -Token $accessToken
  Wait-ContactSearchMiss -ContactId $contactId -ContactName $resolvedContactName -Locale "en-US" -Token $accessToken

  if ($scriptCreatedLinkedCompany -and -not [string]::IsNullOrWhiteSpace($createdLinkedCompanyId)) {
    Write-Log "Deleting temporary linked company fixture created by this script."
    Invoke-GatewayJson `
      -Path "api/companies/$createdLinkedCompanyId" `
      -Method "Delete" `
      -Headers (New-AuthHeaders -Token $accessToken) `
      -AllowedStatusCodes @(200, 202, 204) | Out-Null
  }
} else {
  if ($scriptCreatedLinkedCompany) {
    Write-Log "Delete verification skipped; the temporary linked company is retained because -DeleteAfter was not provided."
  } else {
    Write-Log "Delete verification skipped; pass -DeleteAfter to soft-delete the fixture through the CRM API and verify search removal."
  }
}

Write-Log "Dynamic neutral contact fixture verification completed."
Write-Output ([pscustomobject]@{
    contactId = $contactId
    contactName = $resolvedContactName
    linkedCompanyId = $linkedCompanyId
    linkedCustomerId = $linkedCustomerId
    createdLinkedCompany = $scriptCreatedLinkedCompany
    trLocaleDocumentId = [string]$trHit.id
    enLocaleDocumentId = [string]$enHit.id
    deleteVerified = [bool]$DeleteAfter
  } | ConvertTo-Json -Depth 5)
