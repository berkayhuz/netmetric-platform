param(
  [switch]$PurgeVolumes
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "dev\common.ps1")

$repoRoot = Get-RepoRoot
Set-Location $repoRoot

Write-Log "Stopping tracked local API processes."
Remove-AllTrackedProcesses

Write-Log "Stopping stale dotnet run API processes."
$gatewayProject = Join-Path $repoRoot "platform\gateway\src\NetMetric.ApiGateway\NetMetric.ApiGateway.csproj"
if (Test-Path $gatewayProject) {
  Stop-DotNetRunForProject -ProjectPath $gatewayProject
}

$serviceApiProjects = Get-ChildItem -Path (Join-Path $repoRoot "services") -Recurse -Filter "*.API.csproj" -File -ErrorAction SilentlyContinue
foreach ($project in $serviceApiProjects) {
  Stop-DotNetRunForProject -ProjectPath $project.FullName
}

if ($PurgeVolumes) {
  Write-Log "Stopping docker stack and removing volumes."
  Invoke-Compose -ComposeArgs @("down", "-v", "--remove-orphans")
} else {
  Write-Log "Stopping docker stack."
  Invoke-Compose -ComposeArgs @("down", "--remove-orphans")
}

Write-Log "Dev environment cleaned."
