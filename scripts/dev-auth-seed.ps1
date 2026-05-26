param(
  [string]$MigrationName = "",
  [switch]$SkipCreateMigration
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "dev\common.ps1")

Require-Command -Name docker
Require-Command -Name dotnet

$repoRoot = Get-RepoRoot
Set-Location $repoRoot

$startupProject = Join-Path $repoRoot "services\auth\src\NetMetric.Auth.API\NetMetric.Auth.API.csproj"
$dataProject = Join-Path $repoRoot "services\auth\src\NetMetric.Auth.Infrastructure\NetMetric.Auth.Infrastructure.csproj"
$authConnectionString = "Server=localhost,14333;Database=CRM.AuthDb;User Id=sa;Password=NetMetric.Dev.Sql.2026!;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True"

Write-Log "Restoring local .NET tools."
& dotnet tool restore
if ($LASTEXITCODE -ne 0) {
  throw "dotnet tool restore failed."
}

function Invoke-DotnetEf {
  param([Parameter(ValueFromRemainingArguments = $true)][string[]]$EfArgs)

  & cmd /c "dotnet tool run dotnet-ef --version 1>nul 2>nul"
  if ($LASTEXITCODE -eq 0) {
    & dotnet tool run dotnet-ef @EfArgs
    return
  }

  & cmd /c "dotnet ef --version 1>nul 2>nul"
  if ($LASTEXITCODE -eq 0) {
    & dotnet ef @EfArgs
    return
  }

  throw "dotnet-ef not found after local tool restore. Check .config/dotnet-tools.json and run: dotnet tool restore"
}

if (-not (Test-Path $startupProject)) {
  throw "Startup project not found: $startupProject"
}
if (-not (Test-Path $dataProject)) {
  throw "Data project not found: $dataProject"
}

Write-Log "Ensuring SQL container is running."
Invoke-Compose -ComposeArgs @("up", "-d", "netmetric-sql")

if ([string]::IsNullOrWhiteSpace($MigrationName)) {
  $MigrationName = "Dev_" + (Get-Date -Format "yyyyMMdd_HHmmss")
}

if (-not $SkipCreateMigration) {
  Write-Log "Creating auth migration: $MigrationName"
  Invoke-DotnetEf migrations add $MigrationName `
    --project $dataProject `
    --startup-project $startupProject `
    --context AuthDbContext `
    --no-build
  if ($LASTEXITCODE -ne 0) {
    throw "dotnet ef migrations add failed."
  }
}

Write-Log "Applying auth migration to database."
$env:ConnectionStrings__IdentityConnection = $authConnectionString
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DOTNET_ENVIRONMENT = "Development"
$env:LocalDevelopment__DisableHttpsRedirection = "true"

Invoke-DotnetEf database update `
  --project $dataProject `
  --startup-project $startupProject `
  --context AuthDbContext `
  --no-build
if ($LASTEXITCODE -ne 0) {
  throw "dotnet ef database update failed."
}

Write-Log "Auth migration and database update completed."
