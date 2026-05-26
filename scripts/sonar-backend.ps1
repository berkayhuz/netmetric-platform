param(
    [string]$ProjectKey = "netmetric_backend",
    [string]$ProjectName = "NetMetric Backend",
    [string]$SonarUrl = "http://localhost:9000",
    [string]$Solution = ".\NetMetric.slnx",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string]$StepName,

        [Parameter(Mandatory = $true)]
        [scriptblock]$Command
    )

    Write-Host ""
    Write-Host "==> $StepName"

    try {
        & $Command
    }
    catch {
        Write-Error "$StepName failed. $($_.Exception.Message)"
        exit 1
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Error "$StepName failed with exit code $LASTEXITCODE."
        exit $LASTEXITCODE
    }
}

function Test-DotNetToolInstalled {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ToolPackageName
    )

    $globalTools = & dotnet tool list --global 2>$null
    $globalExitCode = $LASTEXITCODE

    if ($globalExitCode -eq 0 -and ($globalTools -match "(?m)^\s*$([regex]::Escape($ToolPackageName))\s+")) {
        return $true
    }

    $localTools = & dotnet tool list --local 2>$null
    $localExitCode = $LASTEXITCODE

    if ($localExitCode -eq 0 -and ($localTools -match "(?m)^\s*$([regex]::Escape($ToolPackageName))\s+")) {
        return $true
    }

    return $false
}

if ([string]::IsNullOrWhiteSpace($env:SONAR_TOKEN)) {
    Write-Error "SONAR_TOKEN environment variable is required. Set it first: `$env:SONAR_TOKEN='your-token'"
    exit 1
}

$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnetCommand) {
    Write-Error "dotnet CLI was not found. Install the .NET SDK and make sure dotnet is available in PATH."
    exit 1
}

$resolvedSolution = Resolve-Path -LiteralPath $Solution -ErrorAction SilentlyContinue
if ($null -eq $resolvedSolution) {
    Write-Error "Solution file not found: $Solution"
    exit 1
}

$resolvedSolutionPath = $resolvedSolution.Path

if (-not (Test-DotNetToolInstalled -ToolPackageName "dotnet-sonarscanner")) {
    Write-Error "dotnet-sonarscanner is not installed. Install it with: dotnet tool install --global dotnet-sonarscanner"
    exit 1
}

Invoke-Checked -StepName "dotnet sonarscanner begin" -Command {
    dotnet sonarscanner begin `
        /k:"$ProjectKey" `
        /n:"$ProjectName" `
        /d:sonar.host.url="$SonarUrl" `
        /d:sonar.token="$env:SONAR_TOKEN" `
        /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" `
        /d:sonar.exclusions="**/bin/**,**/obj/**,**/node_modules/**,**/.next/**,**/dist/**,**/build/**,**/coverage/**,**/.turbo/**" `
        /d:sonar.coverage.exclusions="**/Migrations/**,**/*.generated.cs,**/*Generated*.cs,**/*Designer.cs,**/Program.cs"
}

Invoke-Checked -StepName "dotnet restore" -Command {
    dotnet restore $resolvedSolutionPath
}

Invoke-Checked -StepName "dotnet build" -Command {
    dotnet build $resolvedSolutionPath -c $Configuration --no-restore --no-incremental
}

Invoke-Checked -StepName "dotnet test (OpenCover coverage)" -Command {
    dotnet test $resolvedSolutionPath `
        -c $Configuration `
        --no-build `
        --collect:"XPlat Code Coverage" `
        --results-directory ".\TestResults" `
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
}

Invoke-Checked -StepName "dotnet sonarscanner end" -Command {
    dotnet sonarscanner end /d:sonar.token="$env:SONAR_TOKEN"
}

Write-Host ""
Write-Host "SonarQube backend analysis completed successfully."