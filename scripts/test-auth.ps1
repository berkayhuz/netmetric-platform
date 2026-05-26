param(
    [switch]$Restore
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

if ($Restore) {
    Write-Host "[auth-tests] Restoring solution..."
    dotnet restore NetMetric.slnx
}

Write-Host "[auth-tests] Building solution (Release)..."
dotnet build NetMetric.slnx -c Release --no-restore -m:1 -v minimal

Write-Host "[auth-tests] Running auth test suite with coverage..."
dotnet test services/auth/tests -c Release --no-build --collect:"XPlat Code Coverage" --results-directory TestResults -m:1 -v minimal

Write-Host "[auth-tests] Completed successfully."
