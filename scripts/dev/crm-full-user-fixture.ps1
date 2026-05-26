param(
  [string]$GatewayBaseUrl = "http://localhost:5030",
  [string]$Password = $env:NETMETRIC_DEV_SEED_PASSWORD,
  [string]$Email = "crm-full-dev@netmetric.local",
  [string]$UserName = "crm-full-dev",
  [string]$TenantName = "NetMetric CRM Full Fixture Tenant",
  [string]$TokenPath = "",
  [string]$SummaryOutputPath = "",
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

function Assert-LocalGatewayUrl {
  param([Parameter(Mandatory = $true)][string]$Url)

  $uri = [Uri]$Url
  $localHosts = @("localhost", "127.0.0.1", "::1")
  if ($uri.Scheme -ne "http" -or $uri.Host -notin $localHosts) {
    throw "Refusing to run CRM full fixture against non-local gateway URL: $Url"
  }
}

function Invoke-Step {
  param(
    [Parameter(Mandatory = $true)][string]$Name,
    [Parameter(Mandatory = $true)][scriptblock]$ScriptBlock
  )

  Write-Log "START $Name"
  & $ScriptBlock
  Write-Log "PASS $Name"
}

function Convert-FixtureOutputToObject {
  param(
    [Parameter(Mandatory = $true)][string]$FixtureName,
    [Parameter(Mandatory = $true)]$FixtureOutput
  )

  if ($null -eq $FixtureOutput) {
    throw "$FixtureName did not return output."
  }

  $raw = ($FixtureOutput | Out-String).Trim()
  if ([string]::IsNullOrWhiteSpace($raw)) {
    throw "$FixtureName returned an empty output payload."
  }

  try {
    return ($raw | ConvertFrom-Json)
  } catch {
    throw "$FixtureName output is not valid JSON. Raw output: $raw"
  }
}

Assert-LocalGatewayUrl -Url $GatewayBaseUrl

if ([string]::IsNullOrWhiteSpace($Password)) {
  throw "Set NETMETRIC_DEV_SEED_PASSWORD or pass -Password before running CRM full fixture seeding."
}

if ([string]::IsNullOrWhiteSpace($TokenPath)) {
  $TokenPath = Join-Path (Get-DevStateRoot) "crm-full-fixture-auth-token.json"
}

if ([string]::IsNullOrWhiteSpace($SummaryOutputPath)) {
  $SummaryOutputPath = Join-Path (Get-DevStateRoot) "crm-full-fixture-summary.json"
}

$summary = [ordered]@{
  success = $false
  timestampUtc = $null
  gatewayBaseUrl = $GatewayBaseUrl.TrimEnd("/")
  user = [ordered]@{
    email = $Email
    userName = $UserName
    tenantName = $TenantName
    tenantId = $null
    userId = $null
  }
  modules = [ordered]@{}
}

try {
  Set-Location (Get-RepoRoot)

  Invoke-Step -Name "gateway readiness" -ScriptBlock {
    Wait-HttpOk -Url "$($GatewayBaseUrl.TrimEnd('/'))/health/ready" -TimeoutSeconds 120
  }

  Invoke-Step -Name "create/login fixture user" -ScriptBlock {
    & (Join-Path $PSScriptRoot "auth-token.ps1") `
      -GatewayBaseUrl $GatewayBaseUrl `
      -Email $Email `
      -UserName $UserName `
      -TenantName $TenantName `
      -Password $Password `
      -OutputPath $TokenPath

    $tokenPayload = Get-Content -Path $TokenPath -Raw | ConvertFrom-Json
    $summary.user.tenantId = [string]$tokenPayload.tenantId
    $summary.user.userId = [string]$tokenPayload.userId
  }

  $commonFixtureArgs = @{
    GatewayBaseUrl = $GatewayBaseUrl
    TokenPath = $TokenPath
    TimeoutSeconds = $TimeoutSeconds
    PollIntervalSeconds = $PollIntervalSeconds
  }

  $customerOutput = $null
  Invoke-Step -Name "customer fixture" -ScriptBlock {
    $args = @{} + $commonFixtureArgs
    if ($DeleteAfter) { $args.DeleteAfter = $true }
    $customerOutput = & (Join-Path $PSScriptRoot "search-customer-fixture.ps1") @args
    $summary.modules.customer = Convert-FixtureOutputToObject -FixtureName "customer fixture" -FixtureOutput $customerOutput
  }

  $companyOutput = $null
  Invoke-Step -Name "company fixture" -ScriptBlock {
    $args = @{} + $commonFixtureArgs
    if ($DeleteAfter) { $args.DeleteAfter = $true }
    $companyOutput = & (Join-Path $PSScriptRoot "search-company-fixture.ps1") @args
    $summary.modules.company = Convert-FixtureOutputToObject -FixtureName "company fixture" -FixtureOutput $companyOutput
  }

  $contactOutput = $null
  Invoke-Step -Name "contact fixture" -ScriptBlock {
    $args = @{} + $commonFixtureArgs
    if ($DeleteAfter) { $args.DeleteAfter = $true }
    $contactOutput = & (Join-Path $PSScriptRoot "search-contact-fixture.ps1") @args
    $summary.modules.contact = Convert-FixtureOutputToObject -FixtureName "contact fixture" -FixtureOutput $contactOutput
  }

  $dealOutput = $null
  Invoke-Step -Name "deal fixture" -ScriptBlock {
    $args = @{} + $commonFixtureArgs
    if ($DeleteAfter) { $args.DeleteAfter = $true }
    if ($SkipOutboxDiagnostics) { $args.SkipOutboxDiagnostics = $true }
    $dealOutput = & (Join-Path $PSScriptRoot "search-deal-fixture.ps1") @args
    $summary.modules.deal = Convert-FixtureOutputToObject -FixtureName "deal fixture" -FixtureOutput $dealOutput
  }

  $opportunityOutput = $null
  Invoke-Step -Name "opportunity fixture" -ScriptBlock {
    $args = @{} + $commonFixtureArgs
    if ($DeleteAfter) { $args.DeleteAfter = $true }
    if ($SkipOutboxDiagnostics) { $args.SkipOutboxDiagnostics = $true }
    $opportunityOutput = & (Join-Path $PSScriptRoot "search-opportunity-fixture.ps1") @args
    $summary.modules.opportunity = Convert-FixtureOutputToObject -FixtureName "opportunity fixture" -FixtureOutput $opportunityOutput
  }

  $quoteOutput = $null
  Invoke-Step -Name "quote fixture" -ScriptBlock {
    $args = @{} + $commonFixtureArgs
    if ($DeleteAfter) { $args.DeleteAfter = $true }
    if ($SkipOutboxDiagnostics) { $args.SkipOutboxDiagnostics = $true }
    $quoteOutput = & (Join-Path $PSScriptRoot "search-quote-fixture.ps1") @args
    $summary.modules.quote = Convert-FixtureOutputToObject -FixtureName "quote fixture" -FixtureOutput $quoteOutput
  }

  $ticketOutput = $null
  Invoke-Step -Name "ticket fixture" -ScriptBlock {
    $args = @{} + $commonFixtureArgs
    if ($DeleteAfter) { $args.DeleteAfter = $true }
    if ($SkipOutboxDiagnostics) { $args.SkipOutboxDiagnostics = $true }
    $ticketOutput = & (Join-Path $PSScriptRoot "search-ticket-fixture.ps1") @args
    $summary.modules.ticket = Convert-FixtureOutputToObject -FixtureName "ticket fixture" -FixtureOutput $ticketOutput
  }

  $leadOutput = $null
  Invoke-Step -Name "lead fixture" -ScriptBlock {
    $args = @{} + $commonFixtureArgs
    if ($DeleteAfter) { $args.DeleteAfter = $true }
    if ($SkipOutboxDiagnostics) { $args.SkipOutboxDiagnostics = $true }
    $leadOutput = & (Join-Path $PSScriptRoot "search-lead-fixture.ps1") @args
    $summary.modules.lead = Convert-FixtureOutputToObject -FixtureName "lead fixture" -FixtureOutput $leadOutput
  }

  $pipelineOutput = $null
  Invoke-Step -Name "pipeline fixture" -ScriptBlock {
    $args = @{} + $commonFixtureArgs
    if ($DeleteAfter) { $args.DeleteAfter = $true }
    if ($SkipOutboxDiagnostics) { $args.SkipOutboxDiagnostics = $true }
    $pipelineOutput = & (Join-Path $PSScriptRoot "search-pipeline-fixture.ps1") @args
    $summary.modules.pipeline = Convert-FixtureOutputToObject -FixtureName "pipeline fixture" -FixtureOutput $pipelineOutput
  }

  $summary.success = $true
  $summary.timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
  ($summary | ConvertTo-Json -Depth 10) | Set-Content -Path $SummaryOutputPath -Encoding UTF8
  Write-Log "CRM full fixture summary written to: $SummaryOutputPath"
  Write-Output ($summary | ConvertTo-Json -Depth 10)
} catch {
  $summary.success = $false
  $summary.timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
  $summary.errorMessage = $_.Exception.Message

  try {
    ($summary | ConvertTo-Json -Depth 10) | Set-Content -Path $SummaryOutputPath -Encoding UTF8
    Write-Log "Failure summary written to: $SummaryOutputPath"
  } catch {
    Write-Log "Warning: Failed to write failure summary: $($_.Exception.Message)"
  }

  throw
}
