# EF Migration Policy (P0)

This policy is enforced by CI and release gates.

## Scope

- `services/auth/src/NetMetric.Auth.Infrastructure`
- `services/account/src/NetMetric.Account.Persistence`

## Required rules

1. Migration files are versioned in Git.
2. Each scoped migrations directory must contain:
   - at least one timestamped migration `yyyyMMddHHmmss_<name>.cs`
   - a `*ModelSnapshot.cs` file
3. Release gate must fail if required migration artifacts are missing.
4. Migration bundle validation must pass for scoped services.

## Enforcement points

- `scripts/release/validate-ef-migrations.mjs`
- `.github/workflows/dotnet-auth.yml`
- `scripts/release/release-gate.mjs`
