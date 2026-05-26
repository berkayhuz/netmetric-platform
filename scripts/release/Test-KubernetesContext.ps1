[CmdletBinding()]
param(
  [switch]$CreateNamespace
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

Write-Host "[PASS] Current context: $context" -ForegroundColor Green

kubectl get nodes | Out-Null
if ($LASTEXITCODE -ne 0) {
  Fail-NoGo "kubectl cannot reach cluster nodes for current context."
}
Write-Host "[PASS] Cluster node query succeeded." -ForegroundColor Green

$nsExists = $false
kubectl get namespace netmetric 1>$null 2>$null
if ($LASTEXITCODE -eq 0) {
  $nsExists = $true
  Write-Host "[PASS] Namespace 'netmetric' exists." -ForegroundColor Green
}
else {
  if ($CreateNamespace) {
    kubectl create namespace netmetric | Out-Null
    if ($LASTEXITCODE -ne 0) {
      Fail-NoGo "Failed to create namespace 'netmetric'."
    }
    Write-Host "[PASS] Namespace 'netmetric' created." -ForegroundColor Green
  }
  else {
    Write-Host "[SKIP] Namespace 'netmetric' does not exist." -ForegroundColor Yellow
  }
}

kubectl get ns cert-manager 1>$null 2>$null
if ($LASTEXITCODE -eq 0) {
  Write-Host "[PASS] cert-manager namespace is visible." -ForegroundColor Green
}
else {
  Write-Host "[SKIP] cert-manager namespace is not visible from current context." -ForegroundColor Yellow
}

Write-Host "Kubernetes context check completed." -ForegroundColor Green
exit 0
