param(
  [switch]$RunDevSmoke,
  [switch]$SkipSmoke,
  [switch]$SkipMigrationBundle,
  [switch]$SkipSecurityScan,
  [switch]$FailOnWarn,
  [switch]$FailFast,
  [switch]$CiOptionalSmoke,
  [Parameter(ValueFromRemainingArguments = $true)]
  [string[]]$RemainingArgs
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$orchestrator = Join-Path $PSScriptRoot "release-gate.mjs"

if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
  throw "Required command not found: node"
}

if (-not (Test-Path $orchestrator)) {
  throw "Release gate orchestrator not found: $orchestrator"
}

# Support GNU-style flags in addition to PowerShell named switches because CI/docs invoke
# this script as: --fail-on-warn --run-dev-smoke.
$gnuFlagMap = @{
  "--run-dev-smoke"       = "RunDevSmoke"
  "--skip-smoke"          = "SkipSmoke"
  "--skip-migration-bundle" = "SkipMigrationBundle"
  "--skip-security-scan"  = "SkipSecurityScan"
  "--fail-on-warn"        = "FailOnWarn"
  "--fail-fast"           = "FailFast"
  "--ci-optional-smoke"   = "CiOptionalSmoke"
}

foreach ($token in ($RemainingArgs | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })) {
  if (-not $gnuFlagMap.ContainsKey($token)) {
    $allowed = ($gnuFlagMap.Keys | Sort-Object) -join ", "
    throw "Unsupported argument '$token'. Supported GNU-style flags: $allowed"
  }

  $target = $gnuFlagMap[$token]
  switch ($target) {
    "RunDevSmoke" { $RunDevSmoke = $true; continue }
    "SkipSmoke" { $SkipSmoke = $true; continue }
    "SkipMigrationBundle" { $SkipMigrationBundle = $true; continue }
    "SkipSecurityScan" { $SkipSecurityScan = $true; continue }
    "FailOnWarn" { $FailOnWarn = $true; continue }
    "FailFast" { $FailFast = $true; continue }
    "CiOptionalSmoke" { $CiOptionalSmoke = $true; continue }
    default { throw "Internal argument mapping error for '$token'." }
  }
}

$args = @()
if ($RunDevSmoke) { $args += "--run-dev-smoke" }
if ($SkipSmoke) { $args += "--skip-smoke" }
if ($SkipMigrationBundle) { $args += "--skip-migration-bundle" }
if ($SkipSecurityScan) { $args += "--skip-security-scan" }
if ($FailOnWarn) { $args += "--fail-on-warn" }
if ($FailFast) { $args += "--fail-fast" }
if ($CiOptionalSmoke) { $args += "--ci-optional-smoke" }

Push-Location $repoRoot
try {
  & node $orchestrator @args
  $exitCode = $LASTEXITCODE
  if ($null -eq $exitCode) {
    throw "Release gate orchestrator did not provide an exit code."
  }
  if ($exitCode -ne 0) {
    throw "Release gate failed with exit code $exitCode."
  }
  exit 0
}
finally {
  Pop-Location
}
