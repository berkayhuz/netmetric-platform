param(
  [string]$Configuration = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Push-Location $repoRoot
try {
  dotnet test services/auth/tests/NetMetric.Auth.Application.UnitTests/NetMetric.Auth.Application.UnitTests.csproj -c $Configuration --no-build --filter "FullyQualifiedName~LoginCommandHandlerTests.Handle_Should_Require_Mfa_When_User_Has_Mfa_And_No_Challenge_Data|FullyQualifiedName~LoginCommandHandlerTests.Handle_Should_Reject_Invalid_Mfa_Code|FullyQualifiedName~LoginCommandHandlerTests.Handle_Should_Login_With_Valid_Mfa_Code"
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

  dotnet test services/account/tests/NetMetric.Account.Application.UnitTests/NetMetric.Account.Application.UnitTests.csproj -c $Configuration --no-build --filter "FullyQualifiedName~SecurityRevokeAndMfaTests"
  exit $LASTEXITCODE
}
finally {
  Pop-Location
}
