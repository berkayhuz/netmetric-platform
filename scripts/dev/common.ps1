Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-RepoRoot {
  return (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

function Get-DevStateRoot {
  $root = Join-Path (Get-RepoRoot) ".local\dev"
  New-Item -ItemType Directory -Path $root -Force | Out-Null
  return $root
}

function Get-ProcessRegistryPath {
  return (Join-Path (Get-DevStateRoot) "processes.json")
}

function Test-ProcessRegistryTransientError {
  param([Parameter(Mandatory = $true)][Exception]$Exception)

  return $Exception -is [System.IO.IOException] -or
    $Exception -is [System.UnauthorizedAccessException]
}

function Invoke-ProcessRegistryIoWithRetry {
  param(
    [Parameter(Mandatory = $true)][string]$Operation,
    [Parameter(Mandatory = $true)][scriptblock]$ScriptBlock,
    [int]$MaxAttempts = 8,
    [int]$DelayMilliseconds = 200
  )

  $attempt = 1
  while ($attempt -le $MaxAttempts) {
    try {
      return & $ScriptBlock
    } catch {
      if (-not (Test-ProcessRegistryTransientError -Exception $_.Exception)) {
        throw
      }

      if ($attempt -ge $MaxAttempts) {
        throw
      }

      Write-Log "$Operation failed due to a transient process registry file lock (attempt $attempt/$MaxAttempts). Retrying."
      Start-Sleep -Milliseconds $DelayMilliseconds
      $attempt++
    }
  }
}

function Write-Log {
  param([string]$Message)
  Write-Host ("[{0}] {1}" -f (Get-Date -Format "HH:mm:ss"), $Message)
}

function Require-Command {
  param([string]$Name)
  if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
    throw "Required command not found: $Name"
  }
}

function Test-CommandAvailable {
  param([Parameter(Mandatory = $true)][string]$Name)
  return $null -ne (Get-Command $Name -ErrorAction SilentlyContinue)
}

function Test-IsWindowsHost {
  return $env:OS -eq "Windows_NT"
}

function Test-DockerReady {
  if (-not (Test-CommandAvailable -Name "docker")) {
    return $false
  }

  $previousErrorActionPreference = $ErrorActionPreference
  try {
    $ErrorActionPreference = "Continue"
    & docker info 1>$null 2>$null
    return $LASTEXITCODE -eq 0
  } catch {
    return $false
  } finally {
    $ErrorActionPreference = $previousErrorActionPreference
  }
}

function Start-DockerDesktopIfAvailable {
  if (-not (Test-IsWindowsHost)) {
    return $false
  }

  $candidatePaths = @(
    (Join-Path $env:ProgramFiles "Docker\Docker\Docker Desktop.exe"),
    (Join-Path ${env:ProgramFiles(x86)} "Docker\Docker\Docker Desktop.exe"),
    (Join-Path $env:LOCALAPPDATA "Programs\Docker Desktop\Docker Desktop.exe")
  ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique

  foreach ($path in $candidatePaths) {
    if (Test-Path $path) {
      Write-Log "Starting Docker Desktop"
      Start-Process -FilePath $path -WindowStyle Hidden | Out-Null
      return $true
    }
  }

  return $false
}

function Wait-DockerReady {
  param(
    [int]$TimeoutSeconds = 180
  )

  if (-not (Test-CommandAvailable -Name "docker")) {
    throw "Docker CLI was not found. Install Docker Desktop and make sure docker is available in PATH."
  }

  if (Test-DockerReady) {
    Write-Log "Docker is ready"
    return
  }

  Write-Log "Docker is not ready"

  $desktopStartAttempted = Start-DockerDesktopIfAvailable

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  while ((Get-Date) -lt $deadline) {
    Write-Log "Waiting for Docker engine"
    Start-Sleep -Seconds 3

    if (Test-DockerReady) {
      Write-Log "Docker is ready"
      return
    }
  }

  if ($desktopStartAttempted) {
    throw "Docker Desktop was started, but Docker engine did not become ready within $TimeoutSeconds seconds."
  }

  throw "Docker engine did not become ready within $TimeoutSeconds seconds. Start Docker and retry."
}

function Invoke-Compose {
  param([string[]]$ComposeArgs)
  $repoRoot = Get-RepoRoot
  if ($null -eq $ComposeArgs -or $ComposeArgs.Count -eq 0) {
    throw "Invoke-Compose requires at least one docker compose command argument."
  }

  Wait-DockerReady

  & docker compose -f (Join-Path $repoRoot "docker-compose.dev.yml") @ComposeArgs
  if ($LASTEXITCODE -ne 0) {
    throw "docker compose failed: $($ComposeArgs -join ' ')"
  }
}

function Remove-ConflictingDevContainers {
  $names = @(
    "netmetric-sql",
    "netmetric-redis",
    "netmetric-rabbitmq",
    "netmetric-meilisearch",
    "netmetric-sonar-db",
    "netmetric-sonarqube"
  )

  foreach ($name in $names) {
    $existingIdRaw = & docker ps -a --filter "name=^/$name$" --format "{{.ID}}"
    $existingId = if ($null -eq $existingIdRaw) { "" } else { ([string]$existingIdRaw).Trim() }
    if (-not [string]::IsNullOrWhiteSpace($existingId)) {
      Write-Log "Removing conflicting container: $name ($existingId)"
      & docker rm -f $name | Out-Null
      if ($LASTEXITCODE -ne 0) {
        throw "Failed to remove conflicting container: $name"
      }
    }
  }
}

function Wait-HttpOk {
  param(
    [Parameter(Mandatory = $true)][string]$Url,
    [int]$TimeoutSeconds = 120
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  while ((Get-Date) -lt $deadline) {
    try {
      $response = Invoke-WebRequest -Uri $Url -Method Get -UseBasicParsing -TimeoutSec 5 -MaximumRedirection 0
      if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 400) {
        return
      }
    } catch {
      $statusCode = $null
      if ($null -ne $_.Exception -and
          $null -ne $_.Exception.PSObject.Properties["Response"] -and
          $null -ne $_.Exception.Response -and
          $null -ne $_.Exception.Response.PSObject.Properties["StatusCode"]) {
        $statusCode = [int]$_.Exception.Response.StatusCode
      }
      if ($null -ne $statusCode -and $statusCode -ge 200 -and $statusCode -lt 400) {
        return
      }
      Start-Sleep -Seconds 2
      continue
    }
    Start-Sleep -Seconds 2
  }
  throw "Timeout waiting for HTTP endpoint: $Url"
}

function Wait-TcpPort {
  param(
    [Parameter(Mandatory = $true)][string]$Address,
    [Parameter(Mandatory = $true)][int]$Port,
    [int]$TimeoutSeconds = 120
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  while ((Get-Date) -lt $deadline) {
    $client = $null
    try {
      $client = New-Object System.Net.Sockets.TcpClient
      $async = $client.BeginConnect($Address, $Port, $null, $null)
      if ($async.AsyncWaitHandle.WaitOne(2000, $false) -and $client.Connected) {
        $client.EndConnect($async) | Out-Null
        return
      }
    } catch {
      # keep retrying until timeout
    } finally {
      if ($null -ne $client) {
        $client.Dispose()
      }
    }
    Start-Sleep -Seconds 2
  }

  throw "Timeout waiting for TCP endpoint: $Address`:$Port"
}

function Load-ProcessRegistry {
  $path = Get-ProcessRegistryPath
  if (-not (Test-Path $path)) {
    return @()
  }

  try {
    $raw = Invoke-ProcessRegistryIoWithRetry `
      -Operation "Loading process registry" `
      -ScriptBlock { Get-Content -Path $path -Raw }
  } catch {
    Write-Log "Unable to read process registry after retries; continuing with an empty process list. $($_.Exception.Message)"
    return @()
  }

  if ([string]::IsNullOrWhiteSpace($raw)) { return @() }
  $entries = $raw | ConvertFrom-Json
  if ($null -ne $entries.PSObject.Properties["value"]) {
    $entries = $entries.value
  }

  return @($entries | Where-Object { $null -ne $_.PSObject.Properties["Name"] })
}

function Save-ProcessRegistry {
  param([array]$Entries)

  $path = Get-ProcessRegistryPath
  $json = $Entries | ConvertTo-Json -Depth 5

  try {
    Invoke-ProcessRegistryIoWithRetry `
      -Operation "Saving process registry" `
      -ScriptBlock {
        $tempPath = "$path.$([Guid]::NewGuid().ToString('N')).tmp"
        try {
          [System.IO.File]::WriteAllText($tempPath, $json, [System.Text.UTF8Encoding]::new($false))
          Move-Item -Path $tempPath -Destination $path -Force
        } finally {
          if (Test-Path $tempPath) {
            Remove-Item -Path $tempPath -Force -ErrorAction SilentlyContinue
          }
        }
      } | Out-Null
  } catch {
    Write-Log "Unable to save process registry after retries; continuing without updating registry metadata. $($_.Exception.Message)"
  }
}

function Add-ProcessRegistryEntry {
  param(
    [string]$Name,
    [string]$ProjectPath,
    [int]$ProcessId
  )
  $entries = Load-ProcessRegistry
  $entries = @($entries | Where-Object { $_.Name -ne $Name })
  $entries += [pscustomobject]@{
    Name       = $Name
    ProjectPath = $ProjectPath
    ProcessId  = $ProcessId
    StartedAt  = (Get-Date).ToString("o")
  }
  Save-ProcessRegistry -Entries $entries
}

function Remove-AllTrackedProcesses {
  $entries = Load-ProcessRegistry
  foreach ($entry in $entries) {
    try {
      $process = Get-Process -Id ([int]$entry.ProcessId) -ErrorAction SilentlyContinue
      if ($null -ne $process) {
        & taskkill.exe /PID $process.Id /T /F | Out-Null
        Write-Log "Stopped process [$($entry.Name)] pid=$($process.Id)"
      }
    } catch {
      Write-Log "Failed stopping process [$($entry.Name)] pid=$($entry.ProcessId): $($_.Exception.Message)"
    }
  }
  Save-ProcessRegistry -Entries @()
}

function Stop-DotNetRunForProject {
  param(
    [Parameter(Mandatory = $true)][string]$ProjectPath
  )

  $normalized = $ProjectPath.ToLowerInvariant()
  $dotnetProcesses = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue
  if ($null -eq $dotnetProcesses) {
    return
  }

  foreach ($process in $dotnetProcesses) {
    $commandLine = [string]$process.CommandLine
    if ([string]::IsNullOrWhiteSpace($commandLine)) {
      continue
    }

    $commandLineLower = $commandLine.ToLowerInvariant()
    if ($commandLineLower.Contains(" run ") -and $commandLineLower.Contains($normalized)) {
      try {
        & taskkill.exe /PID $process.ProcessId /T /F | Out-Null
        Write-Log "Stopped stale dotnet run process pid=$($process.ProcessId) for project [$ProjectPath]"
      } catch {
        Write-Log "Failed stopping stale dotnet run process pid=$($process.ProcessId): $($_.Exception.Message)"
      }
    }
  }
}

function Start-DotNetProject {
  param(
    [Parameter(Mandatory = $true)][string]$Name,
    [Parameter(Mandatory = $true)][string]$ProjectPath,
    [Parameter(Mandatory = $true)][hashtable]$EnvironmentVariables
  )

  $repoRoot = Get-RepoRoot
  $stateRoot = Get-DevStateRoot
  $logsRoot = Join-Path $stateRoot "logs"
  New-Item -ItemType Directory -Path $logsRoot -Force | Out-Null

  $stdoutLog = Join-Path $logsRoot "$Name.stdout.log"
  $stderrLog = Join-Path $logsRoot "$Name.stderr.log"

  $assignments = @(
    "`$ErrorActionPreference='Stop'"
    "Set-Location '$repoRoot'"
  )
  foreach ($k in $EnvironmentVariables.Keys) {
    $value = [string]$EnvironmentVariables[$k]
    $escaped = $value.Replace("'", "''")
    $assignments += "`$env:$k = '$escaped'"
  }
  $projectArg = $ProjectPath.Replace("'", "''")
  $assignments += "dotnet run --no-launch-profile --project '$projectArg'"
  $cmd = ($assignments -join "; ")

  $process = Start-Process -FilePath "powershell.exe" `
    -ArgumentList @("-NoProfile", "-ExecutionPolicy", "Bypass", "-Command", $cmd) `
    -RedirectStandardOutput $stdoutLog `
    -RedirectStandardError $stderrLog `
    -WindowStyle Hidden `
    -PassThru

  Start-Sleep -Seconds 1
  if ($process.HasExited) {
    $stderr = if (Test-Path $stderrLog) { Get-Content -Path $stderrLog -Raw } else { "" }
    throw "Failed to start $Name. Process exited early. $stderr"
  }

  Add-ProcessRegistryEntry -Name $Name -ProjectPath $ProjectPath -ProcessId $process.Id
  Write-Log "Started $Name pid=$($process.Id)"
}

function Invoke-LocalDevSqlQuery {
  param(
    [Parameter(Mandatory = $true)][string]$DatabaseName,
    [Parameter(Mandatory = $true)][string]$Query
  )

  if (-not (Test-CommandAvailable -Name "sqlcmd")) {
    throw "sqlcmd is not available in PATH."
  }

  $server = "localhost,14333"
  $user = "sa"
  $password = if (-not [string]::IsNullOrWhiteSpace($env:NETMETRIC_DEV_SQL_PASSWORD)) {
    $env:NETMETRIC_DEV_SQL_PASSWORD
  } else {
    "NetMetric.Dev.Sql.2026!"
  }

  $sqlcmdArgs = @("-S", $server, "-U", $user, "-C", "-d", $DatabaseName, "-W", "-s", "|", "-h", "-1", "-Q", $Query)
  if (-not [string]::IsNullOrWhiteSpace($password)) {
    $sqlcmdArgs = @("-P", $password) + $sqlcmdArgs
  }

  $rows = & sqlcmd @sqlcmdArgs 2>&1
  if ($LASTEXITCODE -ne 0) {
    throw "sqlcmd query failed. $($rows -join [Environment]::NewLine)"
  }

  $filtered = @($rows | ForEach-Object { [string]$_ } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
  return ,$filtered
}

function Get-LeadOutboxHealthSnapshot {
  $result = [ordered]@{
    available = $false
    warning = ""
    tableExists = $false
    pendingCount = $null
    retryCount = $null
    deadLetterCount = $null
    processedCount = $null
    oldestPendingAgeSeconds = $null
    recentFailureCount = $null
  }

  try {
    $query = @"
SET NOCOUNT ON;
IF OBJECT_ID(N'[LeadManagementOutboxMessages]', N'U') IS NULL
BEGIN
  SELECT CAST(0 AS int), 'NULL', 'NULL', 'NULL', 'NULL', 'NULL', 'NULL';
END
ELSE
BEGIN
  SELECT
    CAST(1 AS int),
    CAST(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL THEN 1 ELSE 0 END) AS nvarchar(50)),
    CAST(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL AND [AttemptCount] > 0 THEN 1 ELSE 0 END) AS nvarchar(50)),
    CAST(SUM(CASE WHEN [DeadLetteredAtUtc] IS NOT NULL THEN 1 ELSE 0 END) AS nvarchar(50)),
    CAST(SUM(CASE WHEN [ProcessedAtUtc] IS NOT NULL THEN 1 ELSE 0 END) AS nvarchar(50)),
    CAST(MAX(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL THEN DATEDIFF(SECOND, [OccurredAtUtc], SYSUTCDATETIME()) ELSE NULL END) AS nvarchar(50)),
    CAST(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL AND [LastError] IS NOT NULL THEN 1 ELSE 0 END) AS nvarchar(50))
  FROM [LeadManagementOutboxMessages]
  OPTION (RECOMPILE);
END
"@

    $lines = Invoke-LocalDevSqlQuery -DatabaseName "NetMetricCrmLeadManagement" -Query $query
    $dataLine = $null
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
      if ($lines[$i].Contains("|")) {
        $dataLine = $lines[$i]
        break
      }
    }

    if ([string]::IsNullOrWhiteSpace($dataLine)) {
      throw "Unexpected sqlcmd output format for lead outbox snapshot."
    }

    $parts = $dataLine.Split("|")
    if ($parts.Count -lt 7) {
      throw "Unexpected lead outbox snapshot column count."
    }

    $toNullableInt = {
      param([string]$value)
      if ([string]::IsNullOrWhiteSpace($value) -or $value -eq "NULL") {
        return $null
      }
      return [int]$value
    }

    $result.available = $true
    $result.tableExists = ([int]$parts[0] -eq 1)
    $result.pendingCount = & $toNullableInt $parts[1]
    $result.retryCount = & $toNullableInt $parts[2]
    $result.deadLetterCount = & $toNullableInt $parts[3]
    $result.processedCount = & $toNullableInt $parts[4]
    $result.oldestPendingAgeSeconds = & $toNullableInt $parts[5]
    $result.recentFailureCount = & $toNullableInt $parts[6]

    if (-not $result.tableExists) {
      $result.warning = "LeadManagementOutboxMessages table is missing."
    }
  } catch {
    $result.warning = $_.Exception.Message
  }

  return [pscustomobject]$result
}

function Get-DealOutboxHealthSnapshot {
  $result = [ordered]@{
    available = $false
    warning = ""
    tableExists = $false
    pendingCount = $null
    retryCount = $null
    deadLetterCount = $null
    processedCount = $null
    oldestPendingAgeSeconds = $null
    recentFailureCount = $null
  }

  try {
    $query = @"
SET NOCOUNT ON;
IF OBJECT_ID(N'[DealManagementOutboxMessages]', N'U') IS NULL
BEGIN
  SELECT CAST(0 AS int), 'NULL', 'NULL', 'NULL', 'NULL', 'NULL', 'NULL';
END
ELSE
BEGIN
  SELECT
    CAST(1 AS int),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL AND [AttemptCount] > 0 THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [DeadLetteredAtUtc] IS NOT NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NOT NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(MAX(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL THEN DATEDIFF(SECOND, [OccurredAtUtc], SYSUTCDATETIME()) ELSE NULL END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL AND [LastError] IS NOT NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50))
  FROM [DealManagementOutboxMessages]
  OPTION (RECOMPILE);
END
"@

    $lines = Invoke-LocalDevSqlQuery -DatabaseName "NetMetricCrmDealManagement" -Query $query
    $dataLine = $null
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
      if ($lines[$i].Contains("|")) {
        $dataLine = $lines[$i]
        break
      }
    }

    if ([string]::IsNullOrWhiteSpace($dataLine)) {
      throw "Unexpected sqlcmd output format for deal outbox snapshot."
    }

    $parts = $dataLine.Split("|")
    if ($parts.Count -lt 7) {
      throw "Unexpected deal outbox snapshot column count."
    }

    $toNullableInt = {
      param([string]$value)
      if ([string]::IsNullOrWhiteSpace($value) -or $value -eq "NULL") {
        return $null
      }
      return [int]$value
    }

    $result.available = $true
    $result.tableExists = ([int]$parts[0] -eq 1)
    $result.pendingCount = & $toNullableInt $parts[1]
    $result.retryCount = & $toNullableInt $parts[2]
    $result.deadLetterCount = & $toNullableInt $parts[3]
    $result.processedCount = & $toNullableInt $parts[4]
    $result.oldestPendingAgeSeconds = & $toNullableInt $parts[5]
    $result.recentFailureCount = & $toNullableInt $parts[6]

    if (-not $result.tableExists) {
      $result.warning = "DealManagementOutboxMessages table is missing."
    }
  } catch {
    $result.warning = $_.Exception.Message
  }

  return [pscustomobject]$result
}

function Get-OpportunityOutboxHealthSnapshot {
  $result = [ordered]@{
    available = $false
    warning = ""
    tableExists = $false
    pendingCount = $null
    retryCount = $null
    deadLetterCount = $null
    processedCount = $null
    oldestPendingAgeSeconds = $null
    recentFailureCount = $null
  }

  try {
    $query = @"
SET NOCOUNT ON;
IF OBJECT_ID(N'[OpportunityManagementOutboxMessages]', N'U') IS NULL
BEGIN
  SELECT CAST(0 AS int), 'NULL', 'NULL', 'NULL', 'NULL', 'NULL', 'NULL';
END
ELSE
BEGIN
  SELECT
    CAST(1 AS int),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL AND [AttemptCount] > 0 THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [DeadLetteredAtUtc] IS NOT NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NOT NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(MAX(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL THEN DATEDIFF(SECOND, [OccurredAtUtc], SYSUTCDATETIME()) ELSE NULL END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL AND [LastError] IS NOT NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50))
  FROM [OpportunityManagementOutboxMessages]
  OPTION (RECOMPILE);
END
"@

    $lines = Invoke-LocalDevSqlQuery -DatabaseName "NetMetricCrmOpportunityManagement" -Query $query
    $dataLine = $null
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
      if ($lines[$i].Contains("|")) {
        $dataLine = $lines[$i]
        break
      }
    }

    if ([string]::IsNullOrWhiteSpace($dataLine)) {
      throw "Unexpected sqlcmd output format for opportunity outbox snapshot."
    }

    $parts = $dataLine.Split("|")
    if ($parts.Count -lt 7) {
      throw "Unexpected opportunity outbox snapshot column count."
    }

    $toNullableInt = {
      param([string]$value)
      if ([string]::IsNullOrWhiteSpace($value) -or $value -eq "NULL") {
        return $null
      }
      return [int]$value
    }

    $result.available = $true
    $result.tableExists = ([int]$parts[0] -eq 1)
    $result.pendingCount = & $toNullableInt $parts[1]
    $result.retryCount = & $toNullableInt $parts[2]
    $result.deadLetterCount = & $toNullableInt $parts[3]
    $result.processedCount = & $toNullableInt $parts[4]
    $result.oldestPendingAgeSeconds = & $toNullableInt $parts[5]
    $result.recentFailureCount = & $toNullableInt $parts[6]

    if (-not $result.tableExists) {
      $result.warning = "OpportunityManagementOutboxMessages table is missing."
    }
  } catch {
    $result.warning = $_.Exception.Message
  }

  return [pscustomobject]$result
}

function Get-QuoteOutboxHealthSnapshot {
  $result = [ordered]@{
    available = $false
    warning = ""
    tableExists = $false
    pendingCount = $null
    retryCount = $null
    deadLetterCount = $null
    processedCount = $null
    oldestPendingAgeSeconds = $null
    recentFailureCount = $null
  }

  try {
    $query = @"
SET NOCOUNT ON;
IF OBJECT_ID(N'[QuoteManagementOutboxMessages]', N'U') IS NULL
BEGIN
  SELECT CAST(0 AS int), 'NULL', 'NULL', 'NULL', 'NULL', 'NULL', 'NULL';
END
ELSE
BEGIN
  SELECT
    CAST(1 AS int),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL AND [AttemptCount] > 0 THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [DeadLetteredAtUtc] IS NOT NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NOT NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(MAX(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL THEN DATEDIFF(SECOND, [OccurredAtUtc], SYSUTCDATETIME()) ELSE NULL END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL AND [LastError] IS NOT NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50))
  FROM [QuoteManagementOutboxMessages]
  OPTION (RECOMPILE);
END
"@

    $lines = Invoke-LocalDevSqlQuery -DatabaseName "NetMetricCrmQuoteManagement" -Query $query
    $dataLine = $null
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
      if ($lines[$i].Contains("|")) {
        $dataLine = $lines[$i]
        break
      }
    }

    if ([string]::IsNullOrWhiteSpace($dataLine)) {
      throw "Unexpected sqlcmd output format for quote outbox snapshot."
    }

    $parts = $dataLine.Split("|")
    if ($parts.Count -lt 7) {
      throw "Unexpected quote outbox snapshot column count."
    }

    $toNullableInt = {
      param([string]$value)
      if ([string]::IsNullOrWhiteSpace($value) -or $value -eq "NULL") {
        return $null
      }
      return [int]$value
    }

    $result.available = $true
    $result.tableExists = ([int]$parts[0] -eq 1)
    $result.pendingCount = & $toNullableInt $parts[1]
    $result.retryCount = & $toNullableInt $parts[2]
    $result.deadLetterCount = & $toNullableInt $parts[3]
    $result.processedCount = & $toNullableInt $parts[4]
    $result.oldestPendingAgeSeconds = & $toNullableInt $parts[5]
    $result.recentFailureCount = & $toNullableInt $parts[6]

    if (-not $result.tableExists) {
      $result.warning = "QuoteManagementOutboxMessages table is missing."
    }
  } catch {
    $result.warning = $_.Exception.Message
  }

  return [pscustomobject]$result
}

function Get-PipelineOutboxHealthSnapshot {
  $result = [ordered]@{
    available = $false
    warning = ""
    tableExists = $false
    pendingCount = $null
    retryCount = $null
    deadLetterCount = $null
    processedCount = $null
    oldestPendingAgeSeconds = $null
    recentFailureCount = $null
  }

  try {
    $query = @"
SET NOCOUNT ON;
IF OBJECT_ID(N'[PipelineManagementOutboxMessages]', N'U') IS NULL
BEGIN
  SELECT CAST(0 AS int), 'NULL', 'NULL', 'NULL', 'NULL', 'NULL', 'NULL';
END
ELSE
BEGIN
  SELECT
    CAST(1 AS int),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL AND [AttemptCount] > 0 THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [DeadLetteredAtUtc] IS NOT NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NOT NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50)),
    CAST(COALESCE(MAX(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL THEN DATEDIFF(SECOND, [OccurredAtUtc], SYSUTCDATETIME()) ELSE NULL END), 0) AS nvarchar(50)),
    CAST(COALESCE(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL AND [LastError] IS NOT NULL THEN 1 ELSE 0 END), 0) AS nvarchar(50))
  FROM [PipelineManagementOutboxMessages]
  OPTION (RECOMPILE);
END
"@

    $lines = Invoke-LocalDevSqlQuery -DatabaseName "NetMetricCrmPipelineManagement" -Query $query
    $dataLine = $null
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
      if ($lines[$i].Contains("|")) {
        $dataLine = $lines[$i]
        break
      }
    }

    if ([string]::IsNullOrWhiteSpace($dataLine)) {
      throw "Unexpected sqlcmd output format for pipeline outbox snapshot."
    }

    $parts = $dataLine.Split("|")
    if ($parts.Count -lt 7) {
      throw "Unexpected pipeline outbox snapshot column count."
    }

    $toNullableInt = {
      param([string]$value)
      if ([string]::IsNullOrWhiteSpace($value) -or $value -eq "NULL") {
        return $null
      }
      return [int]$value
    }

    $result.available = $true
    $result.tableExists = ([int]$parts[0] -eq 1)
    $result.pendingCount = & $toNullableInt $parts[1]
    $result.retryCount = & $toNullableInt $parts[2]
    $result.deadLetterCount = & $toNullableInt $parts[3]
    $result.processedCount = & $toNullableInt $parts[4]
    $result.oldestPendingAgeSeconds = & $toNullableInt $parts[5]
    $result.recentFailureCount = & $toNullableInt $parts[6]

    if (-not $result.tableExists) {
      $result.warning = "PipelineManagementOutboxMessages table is missing."
    }
  } catch {
    $result.warning = $_.Exception.Message
  }

  return [pscustomobject]$result
}

function Get-TicketOutboxHealthSnapshot {
  $result = [ordered]@{
    available = $false
    warning = ""
    tableExists = $false
    pendingCount = $null
    retryCount = $null
    deadLetterCount = $null
    processedCount = $null
    oldestPendingAgeSeconds = $null
    recentFailureCount = $null
  }

  try {
    $query = @"
SET NOCOUNT ON;
IF OBJECT_ID(N'[TicketManagementOutboxMessages]', N'U') IS NULL
BEGIN
  SELECT CAST(0 AS int), 'NULL', 'NULL', 'NULL', 'NULL', 'NULL', 'NULL';
END
ELSE
BEGIN
  SELECT
    CAST(1 AS int),
    CAST(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL THEN 1 ELSE 0 END) AS nvarchar(50)),
    CAST(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL AND [AttemptCount] > 0 THEN 1 ELSE 0 END) AS nvarchar(50)),
    CAST(SUM(CASE WHEN [DeadLetteredAtUtc] IS NOT NULL THEN 1 ELSE 0 END) AS nvarchar(50)),
    CAST(SUM(CASE WHEN [ProcessedAtUtc] IS NOT NULL THEN 1 ELSE 0 END) AS nvarchar(50)),
    CAST(MAX(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL THEN DATEDIFF(SECOND, [OccurredAtUtc], SYSUTCDATETIME()) ELSE NULL END) AS nvarchar(50)),
    CAST(SUM(CASE WHEN [ProcessedAtUtc] IS NULL AND [DeadLetteredAtUtc] IS NULL AND [LastError] IS NOT NULL THEN 1 ELSE 0 END) AS nvarchar(50))
  FROM [TicketManagementOutboxMessages]
  OPTION (RECOMPILE);
END
"@

    $lines = Invoke-LocalDevSqlQuery -DatabaseName "NetMetricCrmTicketManagement" -Query $query
    $dataLine = $null
    for ($i = $lines.Count - 1; $i -ge 0; $i--) {
      if ($lines[$i].Contains("|")) {
        $dataLine = $lines[$i]
        break
      }
    }

    if ([string]::IsNullOrWhiteSpace($dataLine)) {
      throw "Unexpected sqlcmd output format for outbox snapshot."
    }

    $parts = $dataLine.Split("|")
    if ($parts.Count -lt 7) {
      throw "Unexpected outbox snapshot column count."
    }

    $toNullableInt = {
      param([string]$value)
      if ([string]::IsNullOrWhiteSpace($value) -or $value -eq "NULL") {
        return $null
      }
      return [int]$value
    }

    $result.available = $true
    $result.tableExists = ([int]$parts[0] -eq 1)
    $result.pendingCount = & $toNullableInt $parts[1]
    $result.retryCount = & $toNullableInt $parts[2]
    $result.deadLetterCount = & $toNullableInt $parts[3]
    $result.processedCount = & $toNullableInt $parts[4]
    $result.oldestPendingAgeSeconds = & $toNullableInt $parts[5]
    $result.recentFailureCount = & $toNullableInt $parts[6]

    if (-not $result.tableExists) {
      $result.warning = "TicketManagementOutboxMessages table is missing."
    }
  } catch {
    $result.warning = $_.Exception.Message
  }

  return [pscustomobject]$result
}
