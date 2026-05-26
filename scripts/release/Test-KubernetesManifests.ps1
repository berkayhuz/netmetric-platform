[CmdletBinding(SupportsShouldProcess = $true)]
param(
  [string]$Namespace = "netmetric",
  [switch]$ValidateFalse,
  [switch]$Apply,
  [switch]$ConfirmApply,
  [switch]$DryRunOnly = $true
)

$ErrorActionPreference = "Stop"

function Fail-NoGo {
  param([string]$Message)
  Write-Host "[NO-GO] $Message" -ForegroundColor Red
  exit 2
}

if (-not (Get-Command kubectl -ErrorAction SilentlyContinue)) {
  Fail-NoGo "kubectl is not installed or not available in PATH."
}

$context = ""
try {
  $context = (kubectl config current-context 2>$null).Trim()
}
catch {
  $context = ""
}

if ([string]::IsNullOrWhiteSpace($context)) {
  Fail-NoGo "No Kubernetes context is configured."
}

kubectl get nodes 1>$null 2>$null
if ($LASTEXITCODE -ne 0) {
  Fail-NoGo "Cluster is not reachable from current context."
}

$targets = @(
  "deploy/kubernetes/public",
  "deploy/kubernetes/account-web",
  "deploy/kubernetes/tools",
  "deploy/kubernetes/crm-web",`n  "deploy/kubernetes/core"
)

$optionalTargets = @(
  "deploy/kubernetes/auth",
  "deploy/kubernetes/gateway",
  "deploy/kubernetes/api-gateway"
)

foreach ($path in $optionalTargets) {
  if (Test-Path $path) {
    $targets += $path
  }
}

$validateFlag = if ($ValidateFalse) { "--validate=false" } else { "--validate=true" }
$hasFail = $false

Write-Host "=== Kubernetes Manifest Dry-Run ==="
foreach ($target in $targets) {
  if (-not (Test-Path $target)) {
    Write-Host "[SKIP] Missing path: $target" -ForegroundColor Yellow
    continue
  }

  Write-Host "Dry-run: $target"
  $args = @("apply", "--dry-run=client", $validateFlag, "-n", $Namespace, "-f", $target)
  & kubectl @args
  if ($LASTEXITCODE -ne 0) {
    Write-Host "[FAIL] Dry-run failed for $target" -ForegroundColor Red
    $hasFail = $true
  }
  else {
    Write-Host "[PASS] Dry-run passed for $target" -ForegroundColor Green
  }
}

if ($hasFail) {
  Write-Host "Kubernetes manifest validation FAILED." -ForegroundColor Red
  exit 1
}

if (-not $Apply -or $DryRunOnly) {
  Write-Host "Dry-run only mode complete. No resources were applied." -ForegroundColor Green
  exit 0
}

if ($Apply -and -not $ConfirmApply) {
  Fail-NoGo "Apply requested without -ConfirmApply. Refusing mutation."
}

if ($Apply -and $ConfirmApply) {
  Write-Host "=== Kubernetes Manifest Apply ==="
  foreach ($target in $targets) {
    if (-not (Test-Path $target)) {
      Write-Host "[SKIP] Missing path: $target" -ForegroundColor Yellow
      continue
    }

    $applyArgs = @("apply", $validateFlag, "-n", $Namespace, "-f", $target)
    & kubectl @applyArgs
    if ($LASTEXITCODE -ne 0) {
      Write-Host "[FAIL] Apply failed for $target" -ForegroundColor Red
      exit 1
    }

    Write-Host "[PASS] Apply succeeded for $target" -ForegroundColor Green
  }
}

exit 0

