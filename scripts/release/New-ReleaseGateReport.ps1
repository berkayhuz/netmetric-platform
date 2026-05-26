[CmdletBinding()]
param(
  [switch]$IncludeDocker,
  [switch]$IncludeDomainSmoke,
  [string]$BaseDomain = "netmetric.net",
  [string]$Scheme = "https"
)

$ErrorActionPreference = "Stop"

function Invoke-Step {
  param(
    [Parameter(Mandatory = $true)][string]$Title,
    [Parameter(Mandatory = $true)][scriptblock]$Action
  )

  Write-Host ""
  Write-Host "=== $Title ==="
  try {
    & $Action
    if ($LASTEXITCODE -ne 0) {
      Write-Host "[FAIL] $Title (exit=$LASTEXITCODE)" -ForegroundColor Red
      return @{ Status = "FAIL"; ExitCode = $LASTEXITCODE }
    }
    Write-Host "[PASS] $Title" -ForegroundColor Green
    return @{ Status = "PASS"; ExitCode = 0 }
  }
  catch {
    Write-Host "[FAIL] $Title ($($_.Exception.Message))" -ForegroundColor Red
    return @{ Status = "FAIL"; ExitCode = 1 }
  }
}

$results = @{}

Write-Host "Global release gate report started."
Write-Host "This report is non-destructive by default and does not apply Kubernetes manifests."

$results["Local validation guidance"] = Invoke-Step -Title "Local validation guidance" -Action {
  Write-Host "Run before release:"
  Write-Host "  pnpm frontend:typecheck"
  Write-Host "  pnpm frontend:lint"
  Write-Host "  pnpm frontend:build"
  Write-Host "  pnpm run repo:validate"
  Write-Host "  pnpm run repo:format:check"
}

$results["Production config validation"] = Invoke-Step -Title "Production config validation" -Action {
  node ".\\scripts\\release\\validate-production-config.mjs"
}

$results["Kubernetes context"] = Invoke-Step -Title "Kubernetes context" -Action {
  powershell -NoProfile -ExecutionPolicy Bypass -File ".\scripts\release\Test-KubernetesContext.ps1"
}

if ($results["Kubernetes context"].Status -eq "PASS") {
  $results["Kubernetes dry-run"] = Invoke-Step -Title "Kubernetes dry-run" -Action {
    powershell -NoProfile -ExecutionPolicy Bypass -File ".\scripts\release\Test-KubernetesManifests.ps1" -DryRunOnly -ValidateFalse
  }
}
else {
  Write-Host ""
  Write-Host "=== Kubernetes dry-run ==="
  Write-Host "[SKIP] Context check failed; dry-run skipped." -ForegroundColor Yellow
  $results["Kubernetes dry-run"] = @{ Status = "SKIP"; ExitCode = 0 }
}

if ($IncludeDocker) {
  $results["Docker"] = Invoke-Step -Title "Docker" -Action {
    powershell -NoProfile -ExecutionPolicy Bypass -File ".\scripts\release\Test-DockerImages.ps1"
  }
}
else {
  $results["Docker"] = @{ Status = "SKIP"; ExitCode = 0 }
}

if ($IncludeDomainSmoke) {
  $results["Domain smoke"] = Invoke-Step -Title "Domain smoke" -Action {
    powershell -NoProfile -ExecutionPolicy Bypass -File ".\scripts\release\Test-StagingDomains.ps1" -BaseDomain $BaseDomain -Scheme $Scheme
  }
}
else {
  $results["Domain smoke"] = @{ Status = "SKIP"; ExitCode = 0 }
}

Write-Host ""
Write-Host "=== DNS/TLS ==="
Write-Host "Manual checks:"
Write-Host "  kubectl get ingress -n netmetric"
Write-Host "  kubectl get certificate -A"
Write-Host "  kubectl get challenges -A"
Write-Host "  kubectl get orders -A"
Write-Host "  nslookup netmetric.net / account|tools|crm|auth subdomains"
Write-Host "  curl -I https://<domain>"

Write-Host ""
Write-Host "=== Auth/cookie ==="
Write-Host "Manual checks:"
Write-Host "  - Unauthenticated account/tools/crm route redirects to auth with safe returnUrl"
Write-Host "  - Login returns safely to requesting subdomain"
Write-Host "  - Cross-subdomain cookie behavior is compatible with bridge assumptions"
Write-Host "  - No token or cookie values exposed in UI/errors"

$failed = $results.GetEnumerator() | Where-Object { $_.Value.Status -eq "FAIL" }
$hardRequirementsMet = @(
  $results["Kubernetes context"].Status -eq "PASS",
  $results["Kubernetes dry-run"].Status -eq "PASS"
) -notcontains $false

Write-Host ""
Write-Host "=== Final decision ==="
if ($failed.Count -gt 0) {
  Write-Host "[NO-GO] One or more required checks failed." -ForegroundColor Red
  foreach ($f in $failed) {
    Write-Host ("- {0}: {1}" -f $f.Key, $f.Value.Status)
  }
  exit 1
}

if (-not $hardRequirementsMet) {
  Write-Host "[NO-GO] Required Kubernetes checks are not fully confirmed." -ForegroundColor Red
  exit 2
}

Write-Host "[GO-CANDIDATE] Automated non-destructive checks passed." -ForegroundColor Green
Write-Host "Production GO still requires manual DNS/TLS/auth smoke completion."
exit 0
