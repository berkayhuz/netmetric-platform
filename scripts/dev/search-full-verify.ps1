param(
  [string]$GatewayBaseUrl = "http://localhost:5030",
  [string]$MeilisearchEndpoint = "http://localhost:7700",
  [string]$SearchApiBaseUrl = "http://localhost:5310",
  [string]$SearchApiReadyUrl = "http://localhost:5310/health/ready",
  [string]$Password = $env:NETMETRIC_DEV_SEED_PASSWORD,
  [string]$Email = "search-dev@netmetric.local",
  [string]$UserName = "search-dev",
  [string]$TenantName = "NetMetric Search Dev Tenant",
  [string]$TokenPath = "",
  [string]$SummaryOutputPath = "",
  [switch]$DeleteAfter,
  [switch]$SkipReseed,
  [switch]$SkipDynamicCustomer,
  [switch]$SkipCustomerFixture,
  [switch]$SkipCompanyFixture,
  [switch]$SkipContactFixture,
  [switch]$SkipDealFixture,
  [switch]$SkipDealCrossWriterFixture,
  [switch]$SkipOpportunityFixture,
  [switch]$SkipQuoteFixture,
  [switch]$SkipTicketFixture,
  [switch]$SkipLeadFixture,
  [switch]$SkipPipelineFixture,
  [switch]$SkipDynamicFixtures,
  [switch]$SkipOutboxDiagnostics,
  [switch]$PrintToken
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "common.ps1")

function Assert-LocalHttpUrl {
  param(
    [Parameter(Mandatory = $true)][string]$Url,
    [Parameter(Mandatory = $true)][string]$Name
  )

  $uri = [Uri]$Url
  $localHosts = @("localhost", "127.0.0.1", "::1")
  if ($uri.Scheme -ne "http" -or $uri.Host -notin $localHosts) {
    throw "Refusing to run full search verification against non-local $Name URL: $Url"
  }
}

function Test-DotNetRunProject {
  param([Parameter(Mandatory = $true)][string]$ProjectNamePattern)

  $processes = @(Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue | Where-Object {
      ([string]$_.CommandLine) -match $ProjectNamePattern
    })

  return $processes.Count -gt 0
}

function Write-RuntimeDiagnostics {
  Write-Log "Checking local runtime diagnostics."

  if (-not (Test-DotNetRunProject -ProjectNamePattern "NetMetric\.Search\.Worker")) {
    Write-Log "Warning: Search worker process was not detected. Dynamic customer verification needs the worker to consume CRM outbox search events."
  }

  if (-not (Test-DotNetRunProject -ProjectNamePattern "NetMetric\.CRM\.API")) {
    Write-Log "Warning: CRM API process was not detected. Customer fixture creation needs the local CRM API."
  }

  $crmOutboxLog = Join-Path (Get-DevStateRoot) "logs\netmetric.crm.api.stdout.log"
  if (Test-Path $crmOutboxLog) {
    $recent = Get-Content -Path $crmOutboxLog -Tail 300 -ErrorAction SilentlyContinue
    if ($recent -match "CustomerManagement outbox processor is disabled") {
      Write-Log "Warning: recent CRM logs mention disabled CustomerManagement outbox. Restart CRM API through dev-up with Search API selected."
    }

    if ($recent -match "TicketManagement outbox processor is disabled") {
      Write-Log "Warning: recent CRM logs mention disabled TicketManagement outbox. Set TicketManagement__Outbox__Enabled=true for local verification."
    }

    if ($recent -match "DealManagement outbox processor is disabled") {
      Write-Log "Warning: recent CRM logs mention disabled DealManagement outbox. Set DealManagement__Outbox__Enabled=true for local verification."
    }

    if ($recent -match "OpportunityManagement outbox processor is disabled") {
      Write-Log "Warning: recent CRM logs mention disabled OpportunityManagement outbox. Set OpportunityManagement__Outbox__Enabled=true for local verification."
    }

    if ($recent -match "QuoteManagement outbox processor is disabled") {
      Write-Log "Warning: recent CRM logs mention disabled QuoteManagement outbox. Set QuoteManagement__Outbox__Enabled=true for local verification."
    }

    if ($recent -match "LeadManagement outbox processor is disabled") {
      Write-Log "Warning: recent CRM logs mention disabled LeadManagement outbox. Set LeadManagement__Outbox__Enabled=true for local verification."
    }

    if ($recent -match "PipelineManagement outbox processor is disabled") {
      Write-Log "Warning: recent CRM logs mention disabled PipelineManagement outbox. Set PipelineManagement__Outbox__Enabled=true for local verification."
    }
  }
}

function Get-AccessTokenFromFile {
  param([Parameter(Mandatory = $true)][string]$Path)

  if (-not (Test-Path $Path)) {
    throw "Token file was not created: $Path"
  }

  $payload = Get-Content -Path $Path -Raw | ConvertFrom-Json
  if ($null -eq $payload.PSObject.Properties["accessToken"] -or [string]::IsNullOrWhiteSpace($payload.accessToken)) {
    throw "Token file did not include accessToken: $Path"
  }

  return [string]$payload.accessToken
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

Assert-LocalHttpUrl -Url $GatewayBaseUrl -Name "gateway"
Assert-LocalHttpUrl -Url $MeilisearchEndpoint -Name "Meilisearch"
Assert-LocalHttpUrl -Url $SearchApiBaseUrl -Name "Search API"
Assert-LocalHttpUrl -Url $SearchApiReadyUrl -Name "Search API readiness"

if ([string]::IsNullOrWhiteSpace($TokenPath)) {
  $TokenPath = Join-Path (Get-DevStateRoot) "search-auth-token.json"
}

if ([string]::IsNullOrWhiteSpace($SummaryOutputPath)) {
  $SummaryOutputPath = Join-Path (Get-DevStateRoot) "search-full-verify-summary.json"
}

if ([string]::IsNullOrWhiteSpace($Password)) {
  throw "Set NETMETRIC_DEV_SEED_PASSWORD or pass -Password before running full local search verification."
}

$scriptRoot = $PSScriptRoot
$currentStep = "initialization"
$summary = [ordered]@{
  tokenGenerated = $false
  reseeded = $false
  staticAndAuthenticatedVerified = $false
  dynamicCustomerVerified = $false
  dynamicCompanyVerified = $false
  dynamicContactVerified = $false
  dynamicDealVerified = $false
  dynamicDealCrossWriterVerified = $false
  dynamicOpportunityVerified = $false
  dynamicQuoteVerified = $false
  dynamicTicketVerified = $false
  dynamicLeadVerified = $false
  dynamicPipelineVerified = $false
  dynamicCustomerDeleted = [bool]$DeleteAfter
  dynamicCompanyDeleted = [bool]$DeleteAfter
  dynamicContactDeleted = [bool]$DeleteAfter
  dynamicDealDeleted = [bool]$DeleteAfter
  dynamicDealCrossWriterDeleted = [bool]$DeleteAfter
  dynamicOpportunityDeleted = [bool]$DeleteAfter
  dynamicQuoteDeleted = [bool]$DeleteAfter
  dynamicTicketDeleted = [bool]$DeleteAfter
  dynamicLeadDeleted = [bool]$DeleteAfter
  dynamicPipelineDeleted = [bool]$DeleteAfter
  dealOutboxDiagnostics = $null
  opportunityOutboxDiagnostics = $null
  quoteOutboxDiagnostics = $null
  ticketOutboxDiagnostics = $null
  leadOutboxDiagnostics = $null
  pipelineOutboxDiagnostics = $null
  timestampUtc = $null
  success = $false
}

function Write-SummaryFile {
  param(
    [Parameter(Mandatory = $true)]$SummaryObject,
    [Parameter(Mandatory = $true)][string]$OutputPath
  )

  $summaryDirectory = Split-Path -Parent $OutputPath
  if (-not [string]::IsNullOrWhiteSpace($summaryDirectory)) {
    New-Item -ItemType Directory -Path $summaryDirectory -Force | Out-Null
  }

  ($SummaryObject | ConvertTo-Json -Depth 8) | Out-File -FilePath $OutputPath -Encoding utf8
}

try {
  Set-Location (Get-RepoRoot)
  Write-Log "Phase 8I.4 full local search verification starting."
  Write-Log "Expected startup: pnpm dev:up:nobuild with gateway, auth, CRM API, Search API, Search worker, RabbitMQ, and Meilisearch."
  Write-RuntimeDiagnostics
  Write-Log "Checking CRM API readiness."
  Wait-HttpOk -Url "http://localhost:5246/health/ready" -TimeoutSeconds 180

  $currentStep = "local auth token"
  Invoke-Step -Name "local auth token" -ScriptBlock {
    $authArgs = @{
      GatewayBaseUrl = $GatewayBaseUrl
      Email = $Email
      UserName = $UserName
      TenantName = $TenantName
      Password = $Password
      OutputPath = $TokenPath
    }

    if ($PrintToken) {
      $authArgs.PrintToken = $true
    }

    & (Join-Path $scriptRoot "auth-token.ps1") @authArgs
    $summary.tokenGenerated = $true
  }

  $accessToken = Get-AccessTokenFromFile -Path $TokenPath

  if (-not $SkipReseed) {
    $currentStep = "localized static search reseed"
    Invoke-Step -Name "localized static search reseed" -ScriptBlock {
      & (Join-Path $scriptRoot "search-reseed.ps1") `
        -MeilisearchEndpoint $MeilisearchEndpoint `
        -SearchApiBaseUrl $SearchApiBaseUrl `
        -SearchApiReadyUrl $SearchApiReadyUrl `
        -AccountBearerToken $accessToken `
        -CrmBearerToken $accessToken
      $summary.reseeded = $true
    }
  } else {
    Write-Log "SKIP localized static search reseed because -SkipReseed was provided."
  }

  $currentStep = "anonymous/static/authenticated localized search"
  Invoke-Step -Name "anonymous/static/authenticated localized search" -ScriptBlock {
    & (Join-Path $scriptRoot "search-verify.ps1") `
      -GatewayBaseUrl $GatewayBaseUrl `
      -TokenPath $TokenPath `
      -AccountBearerToken $accessToken `
      -CrmBearerToken $accessToken
    $summary.staticAndAuthenticatedVerified = $true
  }

  if ($SkipDynamicFixtures) {
    Write-Log "SKIP all dynamic fixture checks because -SkipDynamicFixtures was provided."
  } else {
    $effectiveSkipCustomer = $SkipDynamicCustomer -or $SkipCustomerFixture

    if (-not $effectiveSkipCustomer) {
      $currentStep = "dynamic neutral customer fixture"
      Invoke-Step -Name "dynamic neutral customer fixture" -ScriptBlock {
        $fixtureArgs = @{
          GatewayBaseUrl = $GatewayBaseUrl
          TokenPath = $TokenPath
          BearerToken = $accessToken
        }

        if ($DeleteAfter) {
          $fixtureArgs.DeleteAfter = $true
        }

        & (Join-Path $scriptRoot "search-customer-fixture.ps1") @fixtureArgs
        $summary.dynamicCustomerVerified = $true
      }
    } else {
      Write-Log "SKIP dynamic customer verification because -SkipDynamicCustomer or -SkipCustomerFixture was provided."
    }

    if (-not $SkipCompanyFixture) {
      $currentStep = "dynamic neutral company fixture"
      Invoke-Step -Name "dynamic neutral company fixture" -ScriptBlock {
        $fixtureArgs = @{
          GatewayBaseUrl = $GatewayBaseUrl
          TokenPath = $TokenPath
          BearerToken = $accessToken
        }

        if ($DeleteAfter) {
          $fixtureArgs.DeleteAfter = $true
        }

        & (Join-Path $scriptRoot "search-company-fixture.ps1") @fixtureArgs
        $summary.dynamicCompanyVerified = $true
      }
    } else {
      Write-Log "SKIP dynamic company verification because -SkipCompanyFixture was provided."
    }

    if (-not $SkipContactFixture) {
      $currentStep = "dynamic neutral contact fixture"
      Invoke-Step -Name "dynamic neutral contact fixture" -ScriptBlock {
        $fixtureArgs = @{
          GatewayBaseUrl = $GatewayBaseUrl
          TokenPath = $TokenPath
          BearerToken = $accessToken
        }

        if ($DeleteAfter) {
          $fixtureArgs.DeleteAfter = $true
        }

        & (Join-Path $scriptRoot "search-contact-fixture.ps1") @fixtureArgs
        $summary.dynamicContactVerified = $true
      }
    } else {
      Write-Log "SKIP dynamic contact verification because -SkipContactFixture was provided."
    }

    if (-not $SkipDealFixture) {
      $currentStep = "dynamic neutral deal fixture"
      Invoke-Step -Name "dynamic neutral deal fixture" -ScriptBlock {
        $fixtureArgs = @{
          GatewayBaseUrl = $GatewayBaseUrl
          TokenPath = $TokenPath
          BearerToken = $accessToken
        }

        if ($DeleteAfter) {
          $fixtureArgs.DeleteAfter = $true
        }

        if ($SkipOutboxDiagnostics) {
          $fixtureArgs.SkipOutboxDiagnostics = $true
        }

        $dealFixtureRaw = & (Join-Path $scriptRoot "search-deal-fixture.ps1") @fixtureArgs
        if (-not [string]::IsNullOrWhiteSpace($dealFixtureRaw)) {
          try {
            $dealFixtureJson = $dealFixtureRaw | ConvertFrom-Json
            if ($null -ne $dealFixtureJson.PSObject.Properties["outboxDiagnostics"]) {
              $summary.dealOutboxDiagnostics = $dealFixtureJson.outboxDiagnostics
            }
          } catch {
            Write-Log "Warning: unable to parse deal fixture JSON output for diagnostics."
          }
        }

        $summary.dynamicDealVerified = $true
      }
    } else {
      Write-Log "SKIP dynamic deal verification because -SkipDealFixture was provided."
    }

    if (-not $SkipDealCrossWriterFixture) {
      $currentStep = "dynamic neutral deal cross-writer fixture"
      Invoke-Step -Name "dynamic neutral deal cross-writer fixture" -ScriptBlock {
        $fixtureArgs = @{
          GatewayBaseUrl = $GatewayBaseUrl
          TokenPath = $TokenPath
          BearerToken = $accessToken
          CreateViaOpportunityWon = $true
        }

        if ($DeleteAfter) {
          $fixtureArgs.DeleteAfter = $true
        }

        if ($SkipOutboxDiagnostics) {
          $fixtureArgs.SkipOutboxDiagnostics = $true
        }

        & (Join-Path $scriptRoot "search-deal-fixture.ps1") @fixtureArgs | Out-Null
        $summary.dynamicDealCrossWriterVerified = $true
      }
    } else {
      Write-Log "SKIP dynamic deal cross-writer verification because -SkipDealCrossWriterFixture was provided."
    }

    if (-not $SkipOpportunityFixture) {
      $currentStep = "dynamic neutral opportunity fixture"
      Invoke-Step -Name "dynamic neutral opportunity fixture" -ScriptBlock {
        $fixtureArgs = @{
          GatewayBaseUrl = $GatewayBaseUrl
          TokenPath = $TokenPath
          BearerToken = $accessToken
        }

        if ($DeleteAfter) {
          $fixtureArgs.DeleteAfter = $true
        }

        if ($SkipOutboxDiagnostics) {
          $fixtureArgs.SkipOutboxDiagnostics = $true
        }

        $opportunityFixtureRaw = & (Join-Path $scriptRoot "search-opportunity-fixture.ps1") @fixtureArgs
        if (-not [string]::IsNullOrWhiteSpace($opportunityFixtureRaw)) {
          try {
            $opportunityFixtureJson = $opportunityFixtureRaw | ConvertFrom-Json
            if ($null -ne $opportunityFixtureJson.PSObject.Properties["outboxDiagnostics"]) {
              $summary.opportunityOutboxDiagnostics = $opportunityFixtureJson.outboxDiagnostics
            }
          } catch {
            Write-Log "Warning: unable to parse opportunity fixture JSON output for diagnostics."
          }
        }

        $summary.dynamicOpportunityVerified = $true
      }
    } else {
      Write-Log "SKIP dynamic opportunity verification because -SkipOpportunityFixture was provided."
    }

    if (-not $SkipQuoteFixture) {
      $currentStep = "dynamic neutral quote fixture"
      Invoke-Step -Name "dynamic neutral quote fixture" -ScriptBlock {
        $fixtureArgs = @{
          GatewayBaseUrl = $GatewayBaseUrl
          TokenPath = $TokenPath
          BearerToken = $accessToken
        }

        if ($DeleteAfter) {
          $fixtureArgs.DeleteAfter = $true
        }

        if ($SkipOutboxDiagnostics) {
          $fixtureArgs.SkipOutboxDiagnostics = $true
        }

        $quoteFixtureRaw = & (Join-Path $scriptRoot "search-quote-fixture.ps1") @fixtureArgs
        if (-not [string]::IsNullOrWhiteSpace($quoteFixtureRaw)) {
          try {
            $quoteFixtureJson = $quoteFixtureRaw | ConvertFrom-Json
            if ($null -ne $quoteFixtureJson.PSObject.Properties["outboxDiagnostics"]) {
              $summary.quoteOutboxDiagnostics = $quoteFixtureJson.outboxDiagnostics
            }
          } catch {
            Write-Log "Warning: unable to parse quote fixture JSON output for diagnostics."
          }
        }

        $summary.dynamicQuoteVerified = $true
      }
    } else {
      Write-Log "SKIP dynamic quote verification because -SkipQuoteFixture was provided."
    }

    if (-not $SkipTicketFixture) {
      $currentStep = "dynamic neutral ticket fixture"
      Invoke-Step -Name "dynamic neutral ticket fixture" -ScriptBlock {
        $fixtureArgs = @{
          GatewayBaseUrl = $GatewayBaseUrl
          TokenPath = $TokenPath
          BearerToken = $accessToken
        }

        if ($DeleteAfter) {
          $fixtureArgs.DeleteAfter = $true
        }

        if ($SkipOutboxDiagnostics) {
          $fixtureArgs.SkipOutboxDiagnostics = $true
        }

        $ticketFixtureRaw = & (Join-Path $scriptRoot "search-ticket-fixture.ps1") @fixtureArgs
        if (-not [string]::IsNullOrWhiteSpace($ticketFixtureRaw)) {
          try {
            $ticketFixtureJson = $ticketFixtureRaw | ConvertFrom-Json
            if ($null -ne $ticketFixtureJson.PSObject.Properties["outboxDiagnostics"]) {
              $summary.ticketOutboxDiagnostics = $ticketFixtureJson.outboxDiagnostics
            }
          } catch {
            Write-Log "Warning: unable to parse ticket fixture JSON output for diagnostics."
          }
        }

        $summary.dynamicTicketVerified = $true
      }
    } else {
      Write-Log "SKIP dynamic ticket verification because -SkipTicketFixture was provided."
    }

    if (-not $SkipLeadFixture) {
      $currentStep = "dynamic neutral lead fixture"
      Invoke-Step -Name "dynamic neutral lead fixture" -ScriptBlock {
        $fixtureArgs = @{
          GatewayBaseUrl = $GatewayBaseUrl
          TokenPath = $TokenPath
          BearerToken = $accessToken
        }

        if ($DeleteAfter) {
          $fixtureArgs.DeleteAfter = $true
        }

        if ($SkipOutboxDiagnostics) {
          $fixtureArgs.SkipOutboxDiagnostics = $true
        }

        $leadFixtureRaw = & (Join-Path $scriptRoot "search-lead-fixture.ps1") @fixtureArgs
        if (-not [string]::IsNullOrWhiteSpace($leadFixtureRaw)) {
          try {
            $leadFixtureJson = $leadFixtureRaw | ConvertFrom-Json
            if ($null -ne $leadFixtureJson.PSObject.Properties["outboxDiagnostics"]) {
              $summary.leadOutboxDiagnostics = $leadFixtureJson.outboxDiagnostics
            }
          } catch {
            Write-Log "Warning: unable to parse lead fixture JSON output for diagnostics."
          }
        }

        $summary.dynamicLeadVerified = $true
      }
    } else {
      Write-Log "SKIP dynamic lead verification because -SkipLeadFixture was provided."
    }

    if (-not $SkipPipelineFixture) {
      $currentStep = "dynamic neutral pipeline fixture"
      Invoke-Step -Name "dynamic neutral pipeline fixture" -ScriptBlock {
        $fixtureArgs = @{
          GatewayBaseUrl = $GatewayBaseUrl
          TokenPath = $TokenPath
          BearerToken = $accessToken
        }

        if ($DeleteAfter) {
          $fixtureArgs.DeleteAfter = $true
        }

        if ($SkipOutboxDiagnostics) {
          $fixtureArgs.SkipOutboxDiagnostics = $true
        }

        $pipelineFixtureRaw = & (Join-Path $scriptRoot "search-pipeline-fixture.ps1") @fixtureArgs
        if (-not [string]::IsNullOrWhiteSpace($pipelineFixtureRaw)) {
          try {
            $pipelineFixtureJson = $pipelineFixtureRaw | ConvertFrom-Json
            if ($null -ne $pipelineFixtureJson.PSObject.Properties["outboxDiagnostics"]) {
              $summary.pipelineOutboxDiagnostics = $pipelineFixtureJson.outboxDiagnostics
            }
          } catch {
            Write-Log "Warning: unable to parse pipeline fixture JSON output for diagnostics."
          }
        }

        $summary.dynamicPipelineVerified = $true
      }
    } else {
      Write-Log "SKIP dynamic pipeline verification because -SkipPipelineFixture was provided."
    }
  }

  $summary.success = $true
  $summary.timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
  Write-SummaryFile -SummaryObject $summary -OutputPath $SummaryOutputPath
  Write-Log "Summary file written: $SummaryOutputPath"
  Write-Log "Phase 8I.4 full local search verification completed."
  Write-Output ($summary | ConvertTo-Json -Depth 8)
} catch {
  $summary.success = $false
  $summary.timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
  $summary.failedStep = $currentStep
  $summary.errorMessage = $_.Exception.Message

  try {
    Write-SummaryFile -SummaryObject $summary -OutputPath $SummaryOutputPath
    Write-Log "Failure summary file written: $SummaryOutputPath"
  } catch {
    Write-Log "Warning: failed to write failure summary file '$SummaryOutputPath': $($_.Exception.Message)"
  }

  throw
}
