param(
  [switch]$SkipBuild,
  [switch]$FullBuild,
  [switch]$Restore,
  [switch]$SkipSonar,
  [switch]$WithSonar,
  [switch]$NoApiStart,
  [switch]$SkipAuthDbReset,
  [switch]$AllApis,

  [string[]]$ApiProjectNames = @(
    "api-gateway",
    "netmetric.crm.api",
    "netmetric.auth.api",
    "netmetric.account.api",
    "netmetric.tools.api",
    "netmetric.search.api"
  ),

  [ValidateSet("Debug", "Release")]
  [string]$Configuration = "Debug",

  [ValidateRange(0, 64)]
  [int]$MaxCpuCount = 0
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. (Join-Path $PSScriptRoot "dev\common.ps1")

function Get-AllApiProjects {
  $repoRoot = Get-RepoRoot
  $projects = @()

  $gatewayProject = Join-Path $repoRoot "platform\gateway\src\NetMetric.ApiGateway\NetMetric.ApiGateway.csproj"
  if (Test-Path $gatewayProject) {
    $projects += [pscustomobject]@{
      Name = "api-gateway"
      Path = $gatewayProject
    }
  }

  $servicesRoot = Join-Path $repoRoot "services"
  if (Test-Path $servicesRoot) {
    $serviceApiProjects = Get-ChildItem `
      -Path $servicesRoot `
      -Recurse `
      -Filter "*.API.csproj" `
      -File `
      -ErrorAction SilentlyContinue

    foreach ($project in $serviceApiProjects) {
      $projects += [pscustomobject]@{
        Name = [IO.Path]::GetFileNameWithoutExtension($project.Name).ToLowerInvariant()
        Path = $project.FullName
      }
    }
  }

  return $projects
}

function Get-SelectedApiProjects {
  param(
    [Parameter(Mandatory = $true)]$AllProjects
  )

  $all = @($AllProjects)

  if ($AllApis) {
    return $all
  }

  $selected = @()

  foreach ($project in $all) {
    foreach ($requestedName in $ApiProjectNames) {
      if ($project.Name -ieq $requestedName) {
        $selected += $project
        break
      }
    }
  }

  foreach ($requestedName in $ApiProjectNames) {
    $exists = $false

    foreach ($project in $all) {
      if ($project.Name -ieq $requestedName) {
        $exists = $true
        break
      }
    }

    if (-not $exists) {
      Write-Log "Requested API project was not found, skipping: $requestedName"
    }
  }

  if (@($selected).Count -eq 0) {
    throw "No API projects selected. Use -AllApis or pass valid -ApiProjectNames."
  }

  return $selected
}

function Test-RestoreAssetsMissing {
  param(
    [Parameter(Mandatory = $true)][object[]]$Projects
  )

  foreach ($project in $Projects) {
    $projectDirectory = Split-Path -Path $project.Path -Parent
    $assetsFile = Join-Path $projectDirectory "obj\project.assets.json"

    if (-not (Test-Path $assetsFile)) {
      Write-Log "Restore assets missing for $($project.Name): $assetsFile"
      return $true
    }
  }

  return $false
}

function Invoke-DotNetRestore {
  param(
    [Parameter(Mandatory = $true)][string]$SolutionPath
  )

  Write-Log "Restoring .NET solution packages."
  & dotnet restore $SolutionPath --locked-mode

  if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore failed."
  }
}

function Invoke-DotNetBuild {
  param(
    [Parameter(Mandatory = $true)][string]$ProjectOrSolution,
    [Parameter(Mandatory = $true)][string]$Configuration,
    [Parameter(Mandatory = $true)][int]$MaxCpuCount
  )

  $buildArgs = @(
    "build",
    $ProjectOrSolution,
    "-c",
    $Configuration,
    "--no-restore",
    "-v",
    "minimal",
    "--nologo",
    "/p:BuildInParallel=true"
  )

  if ($MaxCpuCount -gt 0) {
    $buildArgs += "-m:$MaxCpuCount"
  }
  else {
    $buildArgs += "-m"
  }

  Write-Log "Building [$Configuration]: $ProjectOrSolution"
  & dotnet @buildArgs

  if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed for $ProjectOrSolution."
  }
}

function Ensure-AuthWebEnvLocal {
  $repoRoot = Get-RepoRoot
  $path = Join-Path $repoRoot "apps\auth-web\.env.local"

  if (Test-Path $path) {
    Write-Log "auth-web .env.local already exists, keeping current values."
    return
  }

  @"
NODE_ENV=development
APP_ENV=development
NEXT_PUBLIC_APP_NAME=NetMetric-auth-web
NEXT_PUBLIC_API_GATEWAY_BASE_URL=http://localhost:5030
NEXT_PUBLIC_APP_ORIGIN=http://localhost:7002
"@ | Set-Content -Path $path

  Write-Log "Created apps/auth-web/.env.local for gateway-based local development."
}

function Ensure-FrontendEnvLocal {
  param(
    [Parameter(Mandatory = $true)][string]$AppName
  )

  $repoRoot = Get-RepoRoot
  $appRoot = Join-Path $repoRoot ("apps\" + $AppName)
  $targetPath = Join-Path $appRoot ".env.local"

  if (Test-Path $targetPath) {
    Write-Log "$AppName .env.local already exists, keeping current values."
    return
  }

  $examplePath = Join-Path $appRoot ".env.example"

  if (-not (Test-Path $examplePath)) {
    Write-Log "No .env.example found for $AppName, skipping .env.local bootstrap."
    return
  }

  Copy-Item -Path $examplePath -Destination $targetPath
  Write-Log "Created apps/$AppName/.env.local from .env.example."
}

function Reset-AuthDatabase {
  Write-Log "Resetting auth database (CRM.AuthDb) for deterministic local startup."

  # Local SQL volume can keep orphaned MDF/LDF files even when DB metadata is dropped.
  # Remove them first so CREATE DATABASE does not fail with "file already exists".
  & docker exec -i netmetric-sql /bin/bash -lc "rm -f /var/opt/mssql/data/CRM.AuthDb.mdf /var/opt/mssql/data/CRM.AuthDb_log.ldf" | Out-Null
  if ($LASTEXITCODE -ne 0) {
    throw "Failed to remove orphaned CRM.AuthDb database files from SQL container."
  }

  $sql = @"
IF DB_ID('CRM.AuthDb') IS NOT NULL
BEGIN
  ALTER DATABASE [CRM.AuthDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
  DROP DATABASE [CRM.AuthDb];
END

CREATE DATABASE [CRM.AuthDb];
"@

  & docker exec -i netmetric-sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "NetMetric.Dev.Sql.2026!" -C -Q $sql | Out-Null

  if ($LASTEXITCODE -ne 0) {
    throw "Failed to reset CRM.AuthDb in SQL container."
  }
}

function Ensure-CrmProductCatalogDatabase {
  $dbName = "NetMetricCrmProductCatalog"
  $sqlServer = "localhost,14333"
  $sqlUser = "sa"
  $sqlPassword = "NetMetric.Dev.Sql.2026!"
  $catalogMigrationPath = Join-Path (Get-RepoRoot) "services\crm\src\modules\ProductCatalog\NetMetric.CRM.ProductCatalog.Infrastructure\Persistence\Migrations\20260520_add_catalog_product_pricing_and_category.sql"
  $customerImageMigrationPath = Join-Path (Get-RepoRoot) "services\crm\src\modules\CustomerManagement\NetMetric.CRM.CustomerManagement.Infrastructure\Persistence\Migrations\20260520_add_customer_company_images.sql"

  if (-not (Test-Path $catalogMigrationPath)) {
    throw "Product catalog migration script was not found: $catalogMigrationPath"
  }

  if (-not (Test-Path $customerImageMigrationPath)) {
    throw "Customer image migration script was not found: $customerImageMigrationPath"
  }

  Write-Log "Ensuring Product Catalog database exists: $dbName"
  Invoke-SqlcmdWithRetry `
    -Operation "Ensure Product Catalog database" `
    -Arguments @("-S", $sqlServer, "-U", $sqlUser, "-P", $sqlPassword, "-C", "-Q", "IF DB_ID('$dbName') IS NULL CREATE DATABASE [$dbName];") | Out-Null

  $catalogProductExists = (Invoke-SqlcmdWithRetry `
    -Operation "Check Product Catalog schema" `
    -Arguments @("-S", $sqlServer, "-U", $sqlUser, "-P", $sqlPassword, "-C", "-d", $dbName, "-h", "-1", "-W", "-Q", "SET NOCOUNT ON; SELECT CASE WHEN OBJECT_ID('dbo.CatalogProduct', 'U') IS NULL THEN 0 ELSE 1 END;")).Trim()
  if ($catalogProductExists -eq "1") {
    Write-Log "Applying Product Catalog migration script: 20260520_add_catalog_product_pricing_and_category.sql"
    Invoke-SqlcmdWithRetry `
      -Operation "Apply Product Catalog migration script" `
      -Arguments @("-S", $sqlServer, "-U", $sqlUser, "-P", $sqlPassword, "-C", "-d", $dbName, "-i", $catalogMigrationPath) | Out-Null
  }
  else {
    Write-Log "CatalogProduct table is not present yet. Skipping SQL patch; CRM API development bootstrap will create schema."
  }

  $customerTableExists = (Invoke-SqlcmdWithRetry `
    -Operation "Check legacy Customer Management schema" `
    -Arguments @("-S", $sqlServer, "-U", $sqlUser, "-P", $sqlPassword, "-C", "-d", $dbName, "-h", "-1", "-W", "-Q", "SET NOCOUNT ON; SELECT CASE WHEN OBJECT_ID('dbo.Customers', 'U') IS NULL THEN 0 ELSE 1 END;")).Trim()
  if ($customerTableExists -eq "1") {
    Write-Log "Applying Customer Management migration script: 20260520_add_customer_company_images.sql"
    Invoke-SqlcmdWithRetry `
      -Operation "Apply Customer Management image migration script" `
      -Arguments @("-S", $sqlServer, "-U", $sqlUser, "-P", $sqlPassword, "-C", "-d", $dbName, "-i", $customerImageMigrationPath) | Out-Null
  }
  else {
    Write-Log "Customers table is not present yet. Skipping Customer Management SQL patch; CRM API development bootstrap will create schema."
  }
}

function Ensure-TicketManagementOutboxTable {
  $dbName = "NetMetricCrmTicketManagement"
  $sqlServer = "localhost,14333"
  $sqlUser = "sa"
  $sqlPassword = "NetMetric.Dev.Sql.2026!"
  $sql = @"
IF OBJECT_ID(N'[TicketManagementOutboxMessages]', N'U') IS NULL
BEGIN
  CREATE TABLE [TicketManagementOutboxMessages]
  (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [EventName] nvarchar(256) NOT NULL,
    [EventVersion] int NOT NULL,
    [RoutingKey] nvarchar(256) NOT NULL,
    [PayloadJson] nvarchar(max) NOT NULL,
    [OccurredAtUtc] datetimeoffset NOT NULL,
    [ProcessedAtUtc] datetimeoffset NULL,
    [LockedUntilUtc] datetimeoffset NULL,
    [LockedBy] nvarchar(128) NULL,
    [NextAttemptAtUtc] datetimeoffset NULL,
    [DeadLetteredAtUtc] datetimeoffset NULL,
    [AttemptCount] int NOT NULL CONSTRAINT [DF_TicketManagementOutboxMessages_AttemptCount] DEFAULT (0),
    [LastError] nvarchar(1024) NULL,
    [CorrelationId] nvarchar(128) NULL,
    [IdempotencyKey] nvarchar(160) NULL,
    [Version] rowversion NOT NULL,
    CONSTRAINT [PK_TicketManagementOutboxMessages] PRIMARY KEY ([Id])
  );

  CREATE INDEX [IX_TicketManagementOutboxMessages_Processing]
    ON [TicketManagementOutboxMessages] ([ProcessedAtUtc], [DeadLetteredAtUtc], [NextAttemptAtUtc], [LockedUntilUtc], [OccurredAtUtc]);

  CREATE INDEX [IX_TicketManagementOutboxMessages_Tenant_Event_Occurred]
    ON [TicketManagementOutboxMessages] ([TenantId], [EventName], [OccurredAtUtc]);

  CREATE UNIQUE INDEX [IX_TicketManagementOutboxMessages_IdempotencyKey]
    ON [TicketManagementOutboxMessages] ([IdempotencyKey])
    WHERE [IdempotencyKey] IS NOT NULL;
END
"@

  Write-Log "Ensuring TicketManagement outbox table exists in $dbName."
  Invoke-SqlcmdWithRetry `
    -Operation "Ensure TicketManagement outbox table" `
    -Arguments @("-S", $sqlServer, "-U", $sqlUser, "-P", $sqlPassword, "-C", "-d", $dbName, "-Q", $sql) | Out-Null
}

function Ensure-LeadManagementOutboxTable {
  $dbName = "NetMetricCrmLeadManagement"
  $sqlServer = "localhost,14333"
  $sqlUser = "sa"
  $sqlPassword = "NetMetric.Dev.Sql.2026!"
  $sql = @"
IF OBJECT_ID(N'[LeadManagementOutboxMessages]', N'U') IS NULL
BEGIN
  CREATE TABLE [LeadManagementOutboxMessages]
  (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [EventName] nvarchar(256) NOT NULL,
    [EventVersion] int NOT NULL,
    [RoutingKey] nvarchar(256) NOT NULL,
    [PayloadJson] nvarchar(max) NOT NULL,
    [OccurredAtUtc] datetimeoffset NOT NULL,
    [ProcessedAtUtc] datetimeoffset NULL,
    [LockedUntilUtc] datetimeoffset NULL,
    [LockedBy] nvarchar(128) NULL,
    [NextAttemptAtUtc] datetimeoffset NULL,
    [DeadLetteredAtUtc] datetimeoffset NULL,
    [AttemptCount] int NOT NULL CONSTRAINT [DF_LeadManagementOutboxMessages_AttemptCount] DEFAULT (0),
    [LastError] nvarchar(1024) NULL,
    [CorrelationId] nvarchar(128) NULL,
    [IdempotencyKey] nvarchar(160) NULL,
    [Version] rowversion NOT NULL,
    CONSTRAINT [PK_LeadManagementOutboxMessages] PRIMARY KEY ([Id])
  );

  CREATE INDEX [IX_LeadManagementOutboxMessages_Processing]
    ON [LeadManagementOutboxMessages] ([ProcessedAtUtc], [DeadLetteredAtUtc], [NextAttemptAtUtc], [LockedUntilUtc], [OccurredAtUtc]);

  CREATE INDEX [IX_LeadManagementOutboxMessages_Tenant_Event_Occurred]
    ON [LeadManagementOutboxMessages] ([TenantId], [EventName], [OccurredAtUtc]);

  CREATE UNIQUE INDEX [IX_LeadManagementOutboxMessages_IdempotencyKey]
    ON [LeadManagementOutboxMessages] ([IdempotencyKey])
    WHERE [IdempotencyKey] IS NOT NULL;
END
"@

  Write-Log "Ensuring LeadManagement outbox table exists in $dbName."
  Invoke-SqlcmdWithRetry `
    -Operation "Ensure LeadManagement outbox table" `
    -Arguments @("-S", $sqlServer, "-U", $sqlUser, "-P", $sqlPassword, "-C", "-d", $dbName, "-Q", $sql) | Out-Null
}

function Ensure-DealManagementOutboxTable {
  $dbName = "NetMetricCrmDealManagement"
  $sqlServer = "localhost,14333"
  $sqlUser = "sa"
  $sqlPassword = "NetMetric.Dev.Sql.2026!"
  $sql = @"
IF OBJECT_ID(N'[DealManagementOutboxMessages]', N'U') IS NULL
BEGIN
  CREATE TABLE [DealManagementOutboxMessages]
  (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [EventName] nvarchar(256) NOT NULL,
    [EventVersion] int NOT NULL,
    [RoutingKey] nvarchar(256) NOT NULL,
    [PayloadJson] nvarchar(max) NOT NULL,
    [OccurredAtUtc] datetimeoffset NOT NULL,
    [ProcessedAtUtc] datetimeoffset NULL,
    [LockedUntilUtc] datetimeoffset NULL,
    [LockedBy] nvarchar(128) NULL,
    [NextAttemptAtUtc] datetimeoffset NULL,
    [DeadLetteredAtUtc] datetimeoffset NULL,
    [AttemptCount] int NOT NULL CONSTRAINT [DF_DealManagementOutboxMessages_AttemptCount] DEFAULT (0),
    [LastError] nvarchar(1024) NULL,
    [CorrelationId] nvarchar(128) NULL,
    [IdempotencyKey] nvarchar(160) NULL,
    [Version] rowversion NOT NULL,
    CONSTRAINT [PK_DealManagementOutboxMessages] PRIMARY KEY ([Id])
  );

  CREATE INDEX [IX_DealManagementOutboxMessages_Processing]
    ON [DealManagementOutboxMessages] ([ProcessedAtUtc], [DeadLetteredAtUtc], [NextAttemptAtUtc], [LockedUntilUtc], [OccurredAtUtc]);

  CREATE INDEX [IX_DealManagementOutboxMessages_Tenant_Event_Occurred]
    ON [DealManagementOutboxMessages] ([TenantId], [EventName], [OccurredAtUtc]);

  CREATE UNIQUE INDEX [IX_DealManagementOutboxMessages_IdempotencyKey]
    ON [DealManagementOutboxMessages] ([IdempotencyKey])
    WHERE [IdempotencyKey] IS NOT NULL;
END
"@

  Write-Log "Ensuring DealManagement outbox table exists in $dbName."
  Invoke-SqlcmdWithRetry `
    -Operation "Ensure DealManagement outbox table" `
    -Arguments @("-S", $sqlServer, "-U", $sqlUser, "-P", $sqlPassword, "-C", "-d", $dbName, "-Q", $sql) | Out-Null
}

function Ensure-OpportunityManagementOutboxTable {
  $dbName = "NetMetricCrmOpportunityManagement"
  $sqlServer = "localhost,14333"
  $sqlUser = "sa"
  $sqlPassword = "NetMetric.Dev.Sql.2026!"
  $sql = @"
IF OBJECT_ID(N'[OpportunityManagementOutboxMessages]', N'U') IS NULL
BEGIN
  CREATE TABLE [OpportunityManagementOutboxMessages]
  (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [EventName] nvarchar(256) NOT NULL,
    [EventVersion] int NOT NULL,
    [RoutingKey] nvarchar(256) NOT NULL,
    [PayloadJson] nvarchar(max) NOT NULL,
    [OccurredAtUtc] datetimeoffset NOT NULL,
    [ProcessedAtUtc] datetimeoffset NULL,
    [LockedUntilUtc] datetimeoffset NULL,
    [LockedBy] nvarchar(128) NULL,
    [NextAttemptAtUtc] datetimeoffset NULL,
    [DeadLetteredAtUtc] datetimeoffset NULL,
    [AttemptCount] int NOT NULL CONSTRAINT [DF_OpportunityManagementOutboxMessages_AttemptCount] DEFAULT (0),
    [LastError] nvarchar(1024) NULL,
    [CorrelationId] nvarchar(128) NULL,
    [IdempotencyKey] nvarchar(160) NULL,
    [Version] rowversion NOT NULL,
    CONSTRAINT [PK_OpportunityManagementOutboxMessages] PRIMARY KEY ([Id])
  );

  CREATE INDEX [IX_OpportunityManagementOutboxMessages_Processing]
    ON [OpportunityManagementOutboxMessages] ([ProcessedAtUtc], [DeadLetteredAtUtc], [NextAttemptAtUtc], [LockedUntilUtc], [OccurredAtUtc]);

  CREATE INDEX [IX_OpportunityManagementOutboxMessages_Tenant_Event_Occurred]
    ON [OpportunityManagementOutboxMessages] ([TenantId], [EventName], [OccurredAtUtc]);

  CREATE UNIQUE INDEX [IX_OpportunityManagementOutboxMessages_IdempotencyKey]
    ON [OpportunityManagementOutboxMessages] ([IdempotencyKey])
    WHERE [IdempotencyKey] IS NOT NULL;
END
"@

  Write-Log "Ensuring OpportunityManagement outbox table exists in $dbName."
  Invoke-SqlcmdWithRetry `
    -Operation "Ensure OpportunityManagement outbox table" `
    -Arguments @("-S", $sqlServer, "-U", $sqlUser, "-P", $sqlPassword, "-C", "-d", $dbName, "-Q", $sql) | Out-Null
}

function Ensure-QuoteManagementOutboxTable {
  $dbName = "NetMetricCrmQuoteManagement"
  $sqlServer = "localhost,14333"
  $sqlUser = "sa"
  $sqlPassword = "NetMetric.Dev.Sql.2026!"
  $sql = @"
IF OBJECT_ID(N'[QuoteManagementOutboxMessages]', N'U') IS NULL
BEGIN
  CREATE TABLE [QuoteManagementOutboxMessages]
  (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [EventName] nvarchar(256) NOT NULL,
    [EventVersion] int NOT NULL,
    [RoutingKey] nvarchar(256) NOT NULL,
    [PayloadJson] nvarchar(max) NOT NULL,
    [OccurredAtUtc] datetimeoffset NOT NULL,
    [ProcessedAtUtc] datetimeoffset NULL,
    [LockedUntilUtc] datetimeoffset NULL,
    [LockedBy] nvarchar(128) NULL,
    [NextAttemptAtUtc] datetimeoffset NULL,
    [DeadLetteredAtUtc] datetimeoffset NULL,
    [AttemptCount] int NOT NULL CONSTRAINT [DF_QuoteManagementOutboxMessages_AttemptCount] DEFAULT (0),
    [LastError] nvarchar(1024) NULL,
    [CorrelationId] nvarchar(128) NULL,
    [IdempotencyKey] nvarchar(160) NULL,
    [Version] rowversion NOT NULL,
    CONSTRAINT [PK_QuoteManagementOutboxMessages] PRIMARY KEY ([Id])
  );

  CREATE INDEX [IX_QuoteManagementOutboxMessages_Processing]
    ON [QuoteManagementOutboxMessages] ([ProcessedAtUtc], [DeadLetteredAtUtc], [NextAttemptAtUtc], [LockedUntilUtc], [OccurredAtUtc]);

  CREATE INDEX [IX_QuoteManagementOutboxMessages_Tenant_Event_Occurred]
    ON [QuoteManagementOutboxMessages] ([TenantId], [EventName], [OccurredAtUtc]);

  CREATE UNIQUE INDEX [IX_QuoteManagementOutboxMessages_IdempotencyKey]
    ON [QuoteManagementOutboxMessages] ([IdempotencyKey])
    WHERE [IdempotencyKey] IS NOT NULL;
END
"@

  Write-Log "Ensuring QuoteManagement outbox table exists in $dbName."
  Invoke-SqlcmdWithRetry `
    -Operation "Ensure QuoteManagement outbox table" `
    -Arguments @("-S", $sqlServer, "-U", $sqlUser, "-P", $sqlPassword, "-C", "-d", $dbName, "-Q", $sql) | Out-Null
}

function Ensure-PipelineManagementOutboxTable {
  $dbName = "NetMetricCrmPipelineManagement"
  $sqlServer = "localhost,14333"
  $sqlUser = "sa"
  $sqlPassword = "NetMetric.Dev.Sql.2026!"
  $sql = @"
IF OBJECT_ID(N'[PipelineManagementOutboxMessages]', N'U') IS NULL
BEGIN
  CREATE TABLE [PipelineManagementOutboxMessages]
  (
    [Id] uniqueidentifier NOT NULL,
    [TenantId] uniqueidentifier NOT NULL,
    [EventName] nvarchar(256) NOT NULL,
    [EventVersion] int NOT NULL,
    [RoutingKey] nvarchar(256) NOT NULL,
    [PayloadJson] nvarchar(max) NOT NULL,
    [OccurredAtUtc] datetimeoffset NOT NULL,
    [ProcessedAtUtc] datetimeoffset NULL,
    [LockedUntilUtc] datetimeoffset NULL,
    [LockedBy] nvarchar(128) NULL,
    [NextAttemptAtUtc] datetimeoffset NULL,
    [DeadLetteredAtUtc] datetimeoffset NULL,
    [AttemptCount] int NOT NULL CONSTRAINT [DF_PipelineManagementOutboxMessages_AttemptCount] DEFAULT (0),
    [LastError] nvarchar(1024) NULL,
    [CorrelationId] nvarchar(128) NULL,
    [IdempotencyKey] nvarchar(160) NULL,
    [Version] rowversion NOT NULL,
    CONSTRAINT [PK_PipelineManagementOutboxMessages] PRIMARY KEY ([Id])
  );

  CREATE INDEX [IX_PipelineManagementOutboxMessages_Processing]
    ON [PipelineManagementOutboxMessages] ([ProcessedAtUtc], [DeadLetteredAtUtc], [NextAttemptAtUtc], [LockedUntilUtc], [OccurredAtUtc]);

  CREATE INDEX [IX_PipelineManagementOutboxMessages_Tenant_Event_Occurred]
    ON [PipelineManagementOutboxMessages] ([TenantId], [EventName], [OccurredAtUtc]);

  CREATE UNIQUE INDEX [IX_PipelineManagementOutboxMessages_IdempotencyKey]
    ON [PipelineManagementOutboxMessages] ([IdempotencyKey])
    WHERE [IdempotencyKey] IS NOT NULL;
END
"@

  Write-Log "Ensuring PipelineManagement outbox table exists in $dbName."
  Invoke-SqlcmdWithRetry `
    -Operation "Ensure PipelineManagement outbox table" `
    -Arguments @("-S", $sqlServer, "-U", $sqlUser, "-P", $sqlPassword, "-C", "-d", $dbName, "-Q", $sql) | Out-Null
}

function Invoke-SqlcmdWithRetry {
  param(
    [Parameter(Mandatory = $true)][string[]]$Arguments,
    [string]$Operation = "SQL command",
    [int]$TimeoutSeconds = 120
  )

  $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
  $lastOutput = ""

  while ($true) {
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
      $output = & sqlcmd @Arguments 2>&1
      $exitCode = $LASTEXITCODE
    }
    finally {
      $ErrorActionPreference = $previousErrorActionPreference
    }

    $outputText = ($output | Out-String).Trim()

    if ($exitCode -eq 0) {
      return $outputText
    }

    $lastOutput = $outputText
    if ((Get-Date) -ge $deadline) {
      break
    }

    Write-Log "$Operation is waiting for SQL Server readiness."
    Start-Sleep -Seconds 3
  }

  throw "$Operation failed. $lastOutput"
}

Require-Command -Name docker
Require-Command -Name dotnet
Require-Command -Name pnpm
Require-Command -Name sqlcmd

$repoRoot = Get-RepoRoot
Set-Location $repoRoot

$searchWorkerProject = [pscustomobject]@{
  Name = "netmetric.search.worker"
  Path = (Join-Path $repoRoot "services\search\src\NetMetric.Search.Worker\NetMetric.Search.Worker.csproj")
}

$solutionPath = Join-Path $repoRoot "NetMetric.slnx"
if (-not (Test-Path $solutionPath)) {
  throw "Solution file not found: $solutionPath"
}

$allApiProjects = @(Get-AllApiProjects)
$selectedApiProjects = @(Get-SelectedApiProjects -AllProjects $allApiProjects)

Write-Log "Selected API projects:"
foreach ($project in $selectedApiProjects) {
  Write-Log " - $($project.Name)"
}

Write-Log "Stopping previously tracked local API processes."
Remove-AllTrackedProcesses

Write-Log "Stopping stale dotnet run API processes."
foreach ($project in $allApiProjects) {
  Stop-DotNetRunForProject -ProjectPath $project.Path
}

if (Test-Path $searchWorkerProject.Path) {
  Stop-DotNetRunForProject -ProjectPath $searchWorkerProject.Path
}

Wait-DockerReady

Write-Log "Removing conflicting pre-existing dev containers (if any)."
Remove-ConflictingDevContainers

$composeServices = @(
  "netmetric-sql",
  "netmetric-redis",
  "netmetric-rabbitmq",
  "netmetric-meilisearch"
)

$startSonar = $WithSonar -and (-not $SkipSonar)
if ($startSonar) {
  $composeServices += @("netmetric-sonar-db", "netmetric-sonarqube")
}

Write-Log "Starting docker dependencies."
Invoke-Compose -ComposeArgs (@("up", "-d") + $composeServices)

Write-Log "Waiting for base infrastructure endpoints."
Wait-TcpPort -Address "localhost" -Port 14333 -TimeoutSeconds 180
Wait-TcpPort -Address "localhost" -Port 6379 -TimeoutSeconds 120
Wait-TcpPort -Address "localhost" -Port 5672 -TimeoutSeconds 120
Wait-TcpPort -Address "localhost" -Port 7700 -TimeoutSeconds 120
Wait-HttpOk -Url "http://localhost:15672" -TimeoutSeconds 120
Wait-HttpOk -Url "http://localhost:7700/health" -TimeoutSeconds 120
if ($startSonar) {
  Wait-HttpOk -Url "http://localhost:9000/api/system/status" -TimeoutSeconds 240
}

if (-not $SkipAuthDbReset) {
  Reset-AuthDatabase
}

Ensure-CrmProductCatalogDatabase
Ensure-TicketManagementOutboxTable
Ensure-LeadManagementOutboxTable
Ensure-DealManagementOutboxTable
Ensure-OpportunityManagementOutboxTable
Ensure-QuoteManagementOutboxTable
Ensure-PipelineManagementOutboxTable

Ensure-AuthWebEnvLocal
Ensure-FrontendEnvLocal -AppName "account-web"
Ensure-FrontendEnvLocal -AppName "crm-web"
Ensure-FrontendEnvLocal -AppName "tools-web"
Ensure-FrontendEnvLocal -AppName "public-web"

if (-not $SkipBuild) {
  $restoreRequired = $Restore -or $FullBuild -or (Test-RestoreAssetsMissing -Projects $selectedApiProjects)

  if ($restoreRequired) {
    Invoke-DotNetRestore -SolutionPath $solutionPath
  }
  else {
    Write-Log "Skipping dotnet restore. Use -Restore if package references changed."
  }

  if ($FullBuild) {
    Write-Log "FullBuild enabled. Building complete solution."
    Invoke-DotNetBuild `
      -ProjectOrSolution $solutionPath `
      -Configuration $Configuration `
      -MaxCpuCount $MaxCpuCount
  }
  else {
    Write-Log "Building selected runtime API projects only."

    foreach ($project in $selectedApiProjects) {
      Invoke-DotNetBuild `
        -ProjectOrSolution $project.Path `
        -Configuration $Configuration `
        -MaxCpuCount $MaxCpuCount
    }
  }
}
else {
  Write-Log "Skipping .NET build."
}

if (-not $NoApiStart) {
  $sharedGatewaySecret = "NETMETRIC_DEV_GATEWAY_SIGNING_SECRET_2026_0001"
  $accountInternalSecret = "NETMETRIC_DEV_ACCOUNT_LOCAL_SECRET_2026_0001"

  $authConnectionString = "Server=localhost,14333;Database=CRM.AuthDb;User Id=sa;Password=NetMetric.Dev.Sql.2026!;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True"
  $accountConnectionString = "Server=localhost,14333;Database=CRM.AccountDb;User Id=sa;Password=NetMetric.Dev.Sql.2026!;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True"

  $commonEnv = @{
    ASPNETCORE_ENVIRONMENT                    = "Development"
    DOTNET_ENVIRONMENT                        = "Development"
    LocalDevelopment__DisableHttpsRedirection = "true"
  }

  $selectedNames = @($selectedApiProjects | ForEach-Object { $_.Name })
  $startSearchWorker = ($selectedNames -contains "netmetric.search.api") -and (Test-Path $searchWorkerProject.Path)
  $enableCustomerManagementOutbox = $startSearchWorker

  if ($enableCustomerManagementOutbox) {
    Write-Log "CustomerManagement outbox is enabled for local Search worker verification flow."
  }
  else {
    Write-Log "CustomerManagement outbox remains disabled because Search worker is not part of the selected startup profile."
  }

  foreach ($project in $selectedApiProjects) {
    $envVars = @{}

    foreach ($key in $commonEnv.Keys) {
      $envVars[$key] = $commonEnv[$key]
    }

    if ($project.Name -eq "api-gateway") {
      $envVars["Security__TrustedGateway__Keys__0__Secret"] = $sharedGatewaySecret
    }

    if ($project.Name -match "auth\.api$") {
      $envVars["ConnectionStrings__IdentityConnection"] = $authConnectionString
      $envVars["Messaging__RabbitMq__Uri"] = "amqp://guest:guest@localhost:5672/"
      $envVars["Infrastructure__DistributedCache__Provider"] = "Redis"
      $envVars["Infrastructure__DistributedCache__RedisConnectionString"] = "localhost:6379,abortConnect=false"
      $envVars["Security__TrustedGateway__Keys__0__Secret"] = $sharedGatewaySecret
      $envVars["Security__TrustedGateway__Keys__1__Secret"] = $accountInternalSecret
    }

    if ($project.Name -match "account\.api$") {
      $envVars["ConnectionStrings__AccountDb"] = $accountConnectionString
      $envVars["Messaging__RabbitMq__Uri"] = "amqp://guest:guest@localhost:5672/"
      $envVars["Security__TrustedGateway__Keys__0__Secret"] = $accountInternalSecret
      $envVars["IdentityService__BaseUrl"] = "http://localhost:5297"
      $envVars["MembershipService__BaseUrl"] = "http://localhost:5297"
    }

    if ($project.Name -match "crm\.api$") {
      $crmConnectionStringNames = @(
        "DefaultConnection",
        "AnalyticsReportingConnection",
        "ArtificialIntelligenceConnection",
        "CalendarSyncConnection",
        "ContractLifecycleConnection",
        "CustomerIntelligenceConnection",
        "CustomerManagementConnection",
        "DealManagementConnection",
        "DocumentManagementConnection",
        "FinanceOperationsConnection",
        "IntegrationHubConnection",
        "KnowledgeBaseManagementConnection",
        "LeadManagementConnection",
        "MarketingAutomationConnection",
        "OmnichannelConnection",
        "OpportunityManagementConnection",
        "PipelineManagementConnection",
        "ProductCatalogConnection",
        "QuoteManagementConnection",
        "SalesForecastingConnection",
        "SupportInboxIntegrationConnection",
        "TagManagementConnection",
        "TenantManagementConnection",
        "TicketManagementConnection",
        "TicketSlaManagementConnection",
        "TicketWorkflowManagementConnection",
        "WorkflowAutomationConnection",
        "WorkManagementConnection"
      )

      $envVars["ASPNETCORE_URLS"] = "http://localhost:5246"
      foreach ($connectionName in $crmConnectionStringNames) {
        $databaseSegment = $connectionName -replace "Connection$", ""
        if ($connectionName -eq "DefaultConnection") {
          $databaseSegment = "Default"
        }

        $databaseName = "NetMetricCrm$databaseSegment"
        $envVars["ConnectionStrings__$connectionName"] = "Server=localhost,14333;Database=$databaseName;User Id=sa;Password=NetMetric.Dev.Sql.2026!;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True"
      }
      $envVars["Messaging__RabbitMq__Uri"] = "amqp://guest:guest@localhost:5672/"
      $envVars["CustomerManagement__Outbox__Enabled"] = if ($enableCustomerManagementOutbox) { "true" } else { "false" }
      $envVars["LeadManagement__Outbox__Enabled"] = if ($enableCustomerManagementOutbox) { "true" } else { "false" }
      $envVars["DealManagement__Outbox__Enabled"] = if ($enableCustomerManagementOutbox) { "true" } else { "false" }
      $envVars["OpportunityManagement__Outbox__Enabled"] = if ($enableCustomerManagementOutbox) { "true" } else { "false" }
      $envVars["QuoteManagement__Outbox__Enabled"] = if ($enableCustomerManagementOutbox) { "true" } else { "false" }
      $envVars["PipelineManagement__Outbox__Enabled"] = if ($enableCustomerManagementOutbox) { "true" } else { "false" }
      $envVars["Crm__AnalyticsProjection__Enabled"] = "false"
      $envVars["Crm__Features__IntegrationJobProcessingEnabled"] = "false"
      $envVars["Crm__Features__SupportInboxSyncEnabled"] = "false"
      $envVars["Media__StorageProvider"] = "LocalFile"
      $envVars["Media__Local__RequestPath"] = "/uploads"
      $envVars["Media__Local__RootPath"] = ".runlogs/media"
      $envVars["Media__PublicBaseUrl"] = "http://localhost:5246/uploads"
    }

    if ($project.Name -match "search\.api$") {
      $envVars["ASPNETCORE_URLS"] = "http://localhost:5310"
      $envVars["Search__Provider"] = "Meilisearch"
      $envVars["Search__IndexName"] = "searchdocuments"
      $envVars["Meilisearch__Endpoint"] = "http://localhost:7700"
      $envVars["Meilisearch__ApiKey"] = "NETMETRIC_DEV_MEILI_MASTER_KEY_2026_0001"
      $envVars["Authentication__Jwt__Issuer"] = "http://localhost:5297"
      $envVars["Authentication__Jwt__Audience"] = "http://localhost:5030"
      $envVars["Authentication__Jwt__Authority"] = "http://localhost:5297"
      $envVars["Authentication__Jwt__MetadataAddress"] = "http://localhost:5297/.well-known/openid-configuration"
    }

    Start-DotNetProject `
      -Name $project.Name `
      -ProjectPath $project.Path `
      -EnvironmentVariables $envVars
  }

  if ($startSearchWorker) {
    Write-Log "Starting Search worker process."
    $workerEnv = @{}

    foreach ($key in $commonEnv.Keys) {
      $workerEnv[$key] = $commonEnv[$key]
    }

    $workerEnv["Search__Provider"] = "Meilisearch"
    $workerEnv["Search__IndexName"] = "searchdocuments"
    $workerEnv["Meilisearch__Endpoint"] = "http://localhost:7700"
    $workerEnv["Meilisearch__ApiKey"] = "NETMETRIC_DEV_MEILI_MASTER_KEY_2026_0001"
    $workerEnv["Messaging__RabbitMq__Uri"] = "amqp://guest:guest@localhost:5672/"
    $workerEnv["Messaging__RabbitMq__Exchange"] = "netmetric.search"
    $workerEnv["Search__IntegrationConsumer__Enabled"] = "true"
    $workerEnv["Search__IntegrationConsumer__QueueName"] = "netmetric.search.indexer"
    $workerEnv["Search__IntegrationConsumer__RoutingKeyPatterns__0"] = "search.index.*"
    $workerEnv["Search__IntegrationConsumer__RoutingKeyPatterns__1"] = "search.delete.*"
    $workerEnv["Search__IntegrationConsumer__RoutingKeyPatterns__2"] = "search.reindex.*"

    Start-DotNetProject `
      -Name $searchWorkerProject.Name `
      -ProjectPath $searchWorkerProject.Path `
      -EnvironmentVariables $workerEnv
  }
  elseif (-not (Test-Path $searchWorkerProject.Path)) {
    Write-Log "Search worker project not found, skipping worker startup."
  }

  if ($selectedNames -contains "api-gateway") {
    Write-Log "Waiting for gateway readiness."
    Wait-HttpOk -Url "http://localhost:5030/health/ready" -TimeoutSeconds 120
  }

  if (($selectedNames -contains "api-gateway") -and ($selectedNames -contains "netmetric.auth.api")) {
    Write-Log "Waiting for auth readiness through gateway."
    Wait-HttpOk -Url "http://localhost:5030/auth/health/ready" -TimeoutSeconds 120
  }

  if ($selectedNames -contains "netmetric.crm.api") {
    Write-Log "Waiting for CRM API readiness."
    Wait-HttpOk -Url "http://localhost:5246/health/ready" -TimeoutSeconds 240
  }

  if ($selectedNames -contains "netmetric.search.api") {
    Write-Log "Waiting for search readiness."
    Wait-HttpOk -Url "http://localhost:5310/health/ready" -TimeoutSeconds 120
  }
}
else {
  Write-Log "Skipping API startup."
}

if ($startSonar) {
  Write-Log "SonarQube container is running on http://localhost:9000"
}

Write-Log "Dev environment is up."
