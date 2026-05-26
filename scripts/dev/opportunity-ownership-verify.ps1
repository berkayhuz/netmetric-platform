param(
  [string]$GatewayBaseUrl = "http://localhost:5030",
  [Parameter(Mandatory = $true)][string]$BearerToken,
  [Parameter(Mandatory = $true)][guid]$PipelineConversionLeadId,
  [Parameter(Mandatory = $true)][guid]$LeadManagementConversionLeadId,
  [Nullable[guid]]$PipelineConversionExistingCustomerId = $null,
  [ValidateSet("Prospecting","Qualification","Proposal","Negotiation","Won","Lost")]
  [string]$PipelineStageForMutation = "Qualification"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "common.ps1")

function Invoke-CrmApi {
  param(
    [Parameter(Mandatory = $true)][string]$Method,
    [Parameter(Mandatory = $true)][string]$Path,
    [object]$Body = $null
  )

  $headers = @{
    Authorization = "Bearer $BearerToken"
  }

  $uri = "$($GatewayBaseUrl.TrimEnd('/'))$Path"
  if ($null -eq $Body) {
    return Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers -TimeoutSec 60
  }

  return Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers -ContentType "application/json" -Body ($Body | ConvertTo-Json -Depth 8) -TimeoutSec 60
}

function Assert-OpportunityReadable {
  param([Parameter(Mandatory = $true)][guid]$OpportunityId)
  $opp = Invoke-CrmApi -Method "GET" -Path "/api/opportunities/$OpportunityId"
  if ($null -eq $opp -or [guid]$opp.id -ne $OpportunityId) {
    throw "Opportunity $OpportunityId is not readable from /api/opportunities/{id}."
  }

  return $opp
}

Write-Log "Verifying CRM API readiness."
Wait-HttpOk -Url "$($GatewayBaseUrl.TrimEnd('/'))/health/ready" -TimeoutSeconds 120

Write-Log "1) Creating normal opportunity through OpportunityManagement API."
$createdOpp = Invoke-CrmApi -Method "POST" -Path "/api/opportunities" -Body @{
  opportunityCode = "OPP-OWN-$(Get-Random -Minimum 1000 -Maximum 9999)"
  name = "Ownership Verify Opportunity"
  description = "verification"
  estimatedAmount = 1000
  expectedRevenue = 1200
  probability = 35
  estimatedCloseDate = (Get-Date).ToUniversalTime().AddDays(14).ToString("o")
  stage = "Prospecting"
  status = "Open"
  priority = "Medium"
  leadId = $null
  customerId = $null
  ownerUserId = $null
  notes = $null
}
$normalOpp = Assert-OpportunityReadable -OpportunityId ([guid]$createdOpp.id)

Write-Log "2) Running PipelineManagement lead conversion and asserting Opportunity readability."
$pipelineConversionRequest = @{
  createCustomer = -not $PipelineConversionExistingCustomerId.HasValue
  createOpportunity = $true
  existingCustomerId = if ($PipelineConversionExistingCustomerId.HasValue) { $PipelineConversionExistingCustomerId.Value } else { $null }
  opportunityName = "Pipeline Conversion Ownership Opportunity"
  estimatedAmount = 1500
  initialStage = "Prospecting"
  priority = "Medium"
  ownerUserId = $null
  notes = "verify"
}
$pipelineConversion = Invoke-CrmApi -Method "POST" -Path "/api/pipeline/lead-conversions/$PipelineConversionLeadId" -Body $pipelineConversionRequest
if ($null -eq $pipelineConversion.opportunityId) {
  throw "Pipeline conversion did not return opportunityId."
}
$pipelineOppId = [guid]$pipelineConversion.opportunityId
$pipelineOpp = Assert-OpportunityReadable -OpportunityId $pipelineOppId

Write-Log "3) Running LeadManagement conversion and asserting Opportunity readability."
$leadConversion = Invoke-CrmApi -Method "POST" -Path "/api/leads/$LeadManagementConversionLeadId/convert" -Body @{
  customerType = "Standard"
  markCustomerAsVip = $false
  createOpportunity = $true
  opportunityName = "Lead Conversion Ownership Opportunity"
  estimatedAmount = 1100
  companyId = $null
}
if ($null -eq $leadConversion.opportunityId) {
  throw "Lead conversion did not return opportunityId."
}
$leadOppId = [guid]$leadConversion.opportunityId
$leadOpp = Assert-OpportunityReadable -OpportunityId $leadOppId

Write-Log "4) Mutating pipeline stage and asserting authoritative Opportunity update via /api/opportunities."
$stageMutation = Invoke-CrmApi -Method "POST" -Path "/api/opportunities/pipelines/items/$pipelineOppId/stage" -Body @{
  newStage = $PipelineStageForMutation
  newPipelineStageId = $null
  note = "ownership-stage-verify"
  lostReasonId = $null
  lostNote = $null
  rowVersion = $null
}

$pipelineOppAfter = Assert-OpportunityReadable -OpportunityId $pipelineOppId
if ($pipelineOppAfter.stage -ne $PipelineStageForMutation) {
  throw "Expected stage '$PipelineStageForMutation' on authoritative opportunity, but found '$($pipelineOppAfter.stage)'."
}

$result = [ordered]@{
  normalOpportunityId = $normalOpp.id
  pipelineConversionOpportunityId = $pipelineOpp.id
  leadConversionOpportunityId = $leadOpp.id
  pipelineOpportunityStageAfterMutation = $pipelineOppAfter.stage
  success = $true
  timestampUtc = (Get-Date).ToUniversalTime().ToString("o")
}

Write-Log "Opportunity ownership verification completed."
Write-Output ($result | ConvertTo-Json -Depth 6)
