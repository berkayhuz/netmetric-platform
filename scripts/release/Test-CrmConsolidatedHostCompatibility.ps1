[CmdletBinding()]
param(
  [Alias("BaseUrl")]
  [string]$GatewayBaseUrl = "http://localhost:5030",
  [string]$CrmApiProjectPath = "services/crm/src/NetMetric.CRM.API/NetMetric.CRM.API.csproj",
  [switch]$RouteInventoryOnly,
  [switch]$AllowMutations,
  [int]$TimeoutSeconds = 30,
  [string]$BearerToken
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$gatewayJsonPath = Join-Path $repoRoot "platform/gateway/src/NetMetric.ApiGateway/appsettings.json"
$gatewayDevJsonPath = Join-Path $repoRoot "platform/gateway/src/NetMetric.ApiGateway/appsettings.Development.json"
$gatewayProdJsonPath = Join-Path $repoRoot "platform/gateway/src/NetMetric.ApiGateway/appsettings.Production.json"
$crmControllerRoot = Join-Path $repoRoot "services/crm/src/NetMetric.CRM.API/Controllers/CustomerManagement"

function Write-Pass([string]$Message) { Write-Host "[PASS] $Message" -ForegroundColor Green }
function Write-Fail([string]$Message) { Write-Host "[FAIL] $Message" -ForegroundColor Red }
function Write-Info([string]$Message) { Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Skip([string]$Message) { Write-Host "[SKIP] $Message" -ForegroundColor Yellow }

function New-CorrelationId([string]$suffix) {
  $run = Get-Date -Format "yyyyMMddHHmmss"
  return "crm-smoke-$run-$suffix"
}

function Get-Token() {
  if (-not [string]::IsNullOrWhiteSpace($BearerToken)) { return $BearerToken }
  if (-not [string]::IsNullOrWhiteSpace($env:NETMETRIC_SMOKE_BEARER)) { return $env:NETMETRIC_SMOKE_BEARER }
  return $null
}

function Invoke-CrmJson {
  param(
    [Parameter(Mandatory = $true)][ValidateSet("GET","POST","PUT","DELETE")][string]$Method,
    [Parameter(Mandatory = $true)][string]$Path,
    [string]$Token,
    [object]$Body,
    [switch]$Allow404
  )

  $uri = "$($GatewayBaseUrl.TrimEnd('/'))$Path"
  $headers = @{
    "X-Correlation-Id" = (New-CorrelationId -suffix ([Guid]::NewGuid().ToString("N").Substring(0,8)))
    "Accept" = "application/json"
  }

  if (-not [string]::IsNullOrWhiteSpace($Token)) {
    $headers["Authorization"] = "Bearer $Token"
  }

  try {
    if ($null -ne $Body) {
      $json = $Body | ConvertTo-Json -Depth 8
      $res = Invoke-WebRequest -Uri $uri -Method $Method -Headers $headers -ContentType "application/json" -Body $json -TimeoutSec $TimeoutSeconds -UseBasicParsing
    } else {
      $res = Invoke-WebRequest -Uri $uri -Method $Method -Headers $headers -TimeoutSec $TimeoutSeconds -UseBasicParsing
    }

    $parsed = $null
    if (-not [string]::IsNullOrWhiteSpace($res.Content)) {
      try { $parsed = $res.Content | ConvertFrom-Json -Depth 20 } catch { $parsed = $null }
    }

    return [pscustomobject]@{ Ok = $true; StatusCode = [int]$res.StatusCode; Json = $parsed; Raw = $res.Content }
  }
  catch {
    $resp = $_.Exception.Response
    if ($null -eq $resp) {
      return [pscustomobject]@{ Ok = $false; StatusCode = 0; Json = $null; Raw = $_.Exception.Message }
    }

    $statusCode = [int]$resp.StatusCode
    $bodyText = ""
    try {
      $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
      $bodyText = $sr.ReadToEnd()
      $sr.Dispose()
    } catch {
      $bodyText = "(no response body)"
    }

    if ($Allow404 -and $statusCode -eq 404) {
      return [pscustomobject]@{ Ok = $true; StatusCode = $statusCode; Json = $null; Raw = $bodyText }
    }

    return [pscustomobject]@{ Ok = $false; StatusCode = $statusCode; Json = $null; Raw = $bodyText }
  }
}

function Test-RouteInventory {
  $requiredRoutes = @(
    '[Route("api/customers")]',
    '[HttpGet("{customerId:guid}")]',
    '[HttpPost]',
    '[HttpPut("{customerId:guid}")]',
    '[HttpDelete("{customerId:guid}")]',
    '[Route("api/companies")]',
    '[HttpGet("{companyId:guid}")]',
    '[HttpPut("{companyId:guid}")]',
    '[HttpDelete("{companyId:guid}")]',
    '[Route("api/contacts")]',
    '[HttpGet("{contactId:guid}")]',
    '[HttpPut("{contactId:guid}")]',
    '[HttpDelete("{contactId:guid}")]',
    '[Route("api/addresses")]',
    '[HttpPost("companies/{companyId:guid}")]',
    '[HttpPost("customers/{customerId:guid}")]',
    '[HttpPut("{addressId:guid}")]',
    '[HttpDelete("{addressId:guid}")]'
  )

  $files = Get-ChildItem -Path $crmControllerRoot -Filter "*.cs" -File
  $content = ($files | ForEach-Object { Get-Content $_.FullName -Raw }) -join "`n"
  $missing = @()

  foreach ($route in $requiredRoutes) {
    if ($content -notmatch [Regex]::Escape($route)) { $missing += $route }
  }

  if ($missing.Count -eq 0) {
    Write-Pass "Consolidated CRM route inventory includes required Customers/Companies/Contacts/Addresses actions."
    return $true
  }

  Write-Fail "Missing route annotations in consolidated host: $($missing -join ', ')"
  return $false
}

function Test-GatewayRoutingConfig {
  $gatewayJson = Get-Content $gatewayJsonPath -Raw | ConvertFrom-Json
  $devJson = Get-Content $gatewayDevJsonPath -Raw | ConvertFrom-Json
  $prodJson = Get-Content $gatewayProdJsonPath -Raw | ConvertFrom-Json

  $crmRoute = $gatewayJson.ReverseProxy.Routes."crm-api-route"
  $devClusterDestinations = @($devJson.ReverseProxy.Clusters."crm-api-cluster".Destinations.PSObject.Properties)
  $prodClusterDestinations = @($prodJson.ReverseProxy.Clusters."crm-api-cluster".Destinations.PSObject.Properties)
  $devDest = if (@($devClusterDestinations).Count -gt 0) { $devClusterDestinations[0].Value.Address } else { $null }
  $prodDest = if (@($prodClusterDestinations).Count -gt 0) { $prodClusterDestinations[0].Value.Address } else { $null }

  $ok = $true
  if ($null -eq $crmRoute) {
    Write-Fail "Gateway route 'crm-api-route' not found."
    return $false
  }

  Write-Info ("Gateway CRM path: {0}" -f $crmRoute.Match.Path)
  Write-Info ("Gateway CRM dev destination: {0}" -f $devDest)
  Write-Info ("Gateway CRM prod destination: {0}" -f $prodDest)

  if ($devDest -notmatch "5246") {
    Write-Fail "Development CRM cluster destination is not expected consolidated host port 5246."
    $ok = $false
  }

  if ($prodDest -notmatch "netmetric-crm-api") {
    Write-Fail "Production CRM cluster destination does not point to consolidated crm-api service."
    $ok = $false
  }

  if ($ok) {
    Write-Pass "Gateway routing configuration points CRM traffic to consolidated NetMetric.CRM.API cluster."
  }

  return $ok
}

function Test-ConsolidatedControllerPresence {
  $requiredControllerFiles = @(
    "CustomersController.cs",
    "CompaniesController.cs",
    "ContactsController.cs",
    "AddressesController.cs"
  )

  $ok = $true
  foreach ($file in $requiredControllerFiles) {
    $path = Join-Path $crmControllerRoot $file
    if (-not (Test-Path $path)) {
      Write-Fail "Missing consolidated controller: $file"
      $ok = $false
    } else {
      Write-Pass "Consolidated controller present: $file"
    }
  }

  return $ok
}

function Test-CrmCompatibilitySmoke {
  param([string]$Token)

  if (-not $AllowMutations) {
    Write-Skip "Mutation smoke skipped. Re-run with -AllowMutations and a non-production Gateway base URL."
    return $true
  }

  if ([string]::IsNullOrWhiteSpace($Token)) {
    Write-Skip "No bearer token provided. Set -BearerToken or NETMETRIC_SMOKE_BEARER for automated mutation smoke."
    return $false
  }

  if ($GatewayBaseUrl -match "netmetric.net") {
    Write-Fail "Refusing to run mutation smoke against non-local domain: $GatewayBaseUrl"
    return $false
  }

  $runId = [Guid]::NewGuid().ToString("N").Substring(0, 8)
  $allOk = $true

  $companyId = $null
  $customerId = $null
  $contactId = $null
  $companyAddressId = $null
  $customerAddressId = $null

  $companyPayload = @{
    name = "Smoke Company $runId"
    companyType = "Corporate"
  }

  $customerPayload = @{
    firstName = "Smoke"
    lastName = "Customer $runId"
    customerType = "Individual"
    companyId = $null
  }

  $contactPayload = @{
    firstName = "Smoke"
    lastName = "Contact $runId"
    companyId = $null
    customerId = $null
    isPrimaryContact = $false
  }

  $addressPayload = @{
    addressType = "Billing"
    line1 = "Smoke Line 1"
    city = "Istanbul"
    country = "TR"
    isDefault = $true
  }

  $listCompany = Invoke-CrmJson -Method GET -Path "/api/companies?page=1&pageSize=5" -Token $Token
  if (-not $listCompany.Ok) { Write-Fail "Companies list failed ($($listCompany.StatusCode))"; $allOk = $false } else { Write-Pass "Companies list ok" }

  $createCompany = Invoke-CrmJson -Method POST -Path "/api/companies" -Token $Token -Body $companyPayload
  if (-not $createCompany.Ok -or $createCompany.StatusCode -lt 200 -or $createCompany.StatusCode -ge 300) {
    Write-Fail "Company create failed ($($createCompany.StatusCode))"
    return $false
  }
  $companyId = [string]$createCompany.Json.id
  Write-Pass "Company create/detail/update/delete flow started"

  $companyDetail = Invoke-CrmJson -Method GET -Path "/api/companies/$companyId" -Token $Token
  if (-not $companyDetail.Ok) { Write-Fail "Company detail failed ($($companyDetail.StatusCode))"; $allOk = $false }

  $companyUpdate = $companyPayload.Clone()
  $companyUpdate.name = "Smoke Company Updated $runId"
  if ($createCompany.Json.PSObject.Properties.Name -contains "rowVersion") { $companyUpdate.rowVersion = $createCompany.Json.rowVersion }
  $updCompany = Invoke-CrmJson -Method PUT -Path "/api/companies/$companyId" -Token $Token -Body $companyUpdate
  if (-not $updCompany.Ok) { Write-Fail "Company update failed ($($updCompany.StatusCode))"; $allOk = $false }

  $customerPayload.companyId = $companyId
  $listCustomer = Invoke-CrmJson -Method GET -Path "/api/customers?page=1&pageSize=5" -Token $Token
  if (-not $listCustomer.Ok) { Write-Fail "Customers list failed ($($listCustomer.StatusCode))"; $allOk = $false } else { Write-Pass "Customers list ok" }

  $createCustomer = Invoke-CrmJson -Method POST -Path "/api/customers" -Token $Token -Body $customerPayload
  if (-not $createCustomer.Ok -or $createCustomer.StatusCode -lt 200 -or $createCustomer.StatusCode -ge 300) {
    Write-Fail "Customer create failed ($($createCustomer.StatusCode))"
    return $false
  }
  $customerId = [string]$createCustomer.Json.id

  $customerDetail = Invoke-CrmJson -Method GET -Path "/api/customers/$customerId" -Token $Token
  if (-not $customerDetail.Ok) { Write-Fail "Customer detail failed ($($customerDetail.StatusCode))"; $allOk = $false }

  $customerUpdate = $customerPayload.Clone()
  $customerUpdate.firstName = "SmokeUpdated"
  if ($createCustomer.Json.PSObject.Properties.Name -contains "rowVersion") { $customerUpdate.rowVersion = $createCustomer.Json.rowVersion }
  $updCustomer = Invoke-CrmJson -Method PUT -Path "/api/customers/$customerId" -Token $Token -Body $customerUpdate
  if (-not $updCustomer.Ok) { Write-Fail "Customer update failed ($($updCustomer.StatusCode))"; $allOk = $false }

  $contactPayload.companyId = $companyId
  $contactPayload.customerId = $customerId
  $listContact = Invoke-CrmJson -Method GET -Path "/api/contacts?page=1&pageSize=5" -Token $Token
  if (-not $listContact.Ok) { Write-Fail "Contacts list failed ($($listContact.StatusCode))"; $allOk = $false } else { Write-Pass "Contacts list ok" }

  $createContact = Invoke-CrmJson -Method POST -Path "/api/contacts" -Token $Token -Body $contactPayload
  if (-not $createContact.Ok -or $createContact.StatusCode -lt 200 -or $createContact.StatusCode -ge 300) {
    Write-Fail "Contact create failed ($($createContact.StatusCode))"
    return $false
  }
  $contactId = [string]$createContact.Json.id

  $contactDetail = Invoke-CrmJson -Method GET -Path "/api/contacts/$contactId" -Token $Token
  if (-not $contactDetail.Ok) { Write-Fail "Contact detail failed ($($contactDetail.StatusCode))"; $allOk = $false }

  $contactUpdate = $contactPayload.Clone()
  $contactUpdate.lastName = "ContactUpdated $runId"
  if ($createContact.Json.PSObject.Properties.Name -contains "rowVersion") { $contactUpdate.rowVersion = $createContact.Json.rowVersion }
  $updContact = Invoke-CrmJson -Method PUT -Path "/api/contacts/$contactId" -Token $Token -Body $contactUpdate
  if (-not $updContact.Ok) { Write-Fail "Contact update failed ($($updContact.StatusCode))"; $allOk = $false }

  $companyAddress = Invoke-CrmJson -Method POST -Path "/api/addresses/companies/$companyId" -Token $Token -Body $addressPayload
  if ($companyAddress.Ok) { $companyAddressId = [string]$companyAddress.Json.id; Write-Pass "Company address create ok" } else { Write-Fail "Company address create failed ($($companyAddress.StatusCode))"; $allOk = $false }

  $customerAddress = Invoke-CrmJson -Method POST -Path "/api/addresses/customers/$customerId" -Token $Token -Body $addressPayload
  if ($customerAddress.Ok) { $customerAddressId = [string]$customerAddress.Json.id; Write-Pass "Customer address create ok" } else { Write-Fail "Customer address create failed ($($customerAddress.StatusCode))"; $allOk = $false }

  if ($companyAddressId) {
    $addrUpdate = $addressPayload.Clone()
    $addrUpdate.line1 = "Smoke Updated Line"
    if ($companyAddress.Json.PSObject.Properties.Name -contains "rowVersion") { $addrUpdate.rowVersion = $companyAddress.Json.rowVersion }
    $addrUpd = Invoke-CrmJson -Method PUT -Path "/api/addresses/$companyAddressId" -Token $Token -Body $addrUpdate
    if (-not $addrUpd.Ok) { Write-Fail "Address update failed ($($addrUpd.StatusCode))"; $allOk = $false }
  }

  if ($contactId) {
    $delContact = Invoke-CrmJson -Method DELETE -Path "/api/contacts/$contactId" -Token $Token
    if (-not $delContact.Ok) { Write-Fail "Contact delete failed ($($delContact.StatusCode))"; $allOk = $false }
  }

  if ($customerAddressId) {
    $delCustAddr = Invoke-CrmJson -Method DELETE -Path "/api/addresses/$customerAddressId" -Token $Token
    if (-not $delCustAddr.Ok) { Write-Fail "Customer address delete failed ($($delCustAddr.StatusCode))"; $allOk = $false }
  }

  if ($companyAddressId) {
    $delCompAddr = Invoke-CrmJson -Method DELETE -Path "/api/addresses/$companyAddressId" -Token $Token
    if (-not $delCompAddr.Ok) { Write-Fail "Company address delete failed ($($delCompAddr.StatusCode))"; $allOk = $false }
  }

  if ($customerId) {
    $delCustomer = Invoke-CrmJson -Method DELETE -Path "/api/customers/$customerId" -Token $Token
    if (-not $delCustomer.Ok) { Write-Fail "Customer delete failed ($($delCustomer.StatusCode))"; $allOk = $false }
  }

  if ($companyId) {
    $delCompany = Invoke-CrmJson -Method DELETE -Path "/api/companies/$companyId" -Token $Token
    if (-not $delCompany.Ok) { Write-Fail "Company delete failed ($($delCompany.StatusCode))"; $allOk = $false }
  }

  if ($allOk) {
    Write-Pass "Customers/Companies/Contacts/Addresses mutation smoke passed via Gateway -> consolidated CRM host."
  }

  return $allOk
}

Push-Location $repoRoot
try {
  Write-Info "Phase C CRM consolidation smoke started. Gateway=$GatewayBaseUrl"

  $okRoute = Test-RouteInventory
  $okGateway = Test-GatewayRoutingConfig
  $okControllers = Test-ConsolidatedControllerPresence

  if ($RouteInventoryOnly) {
    if ($okRoute -and $okGateway -and $okControllers) { exit 0 }
    exit 1
  }

  $token = Get-Token
  if (-not [string]::IsNullOrWhiteSpace($token)) {
    Write-Info "Bearer token provided (redacted)."
  } else {
    Write-Info "No bearer token provided. Automated mutation smoke requires token."
  }

  $okSmoke = Test-CrmCompatibilitySmoke -Token $token

  if ($okRoute -and $okGateway -and $okControllers -and $okSmoke) {
    Write-Pass "CRM consolidated host compatibility smoke PASSED."
    exit 0
  }

  Write-Fail "CRM consolidated host compatibility smoke FAILED or INCOMPLETE."
  exit 1
}
finally {
  Pop-Location
}

