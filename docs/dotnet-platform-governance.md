# .NET Platform Governance

## Repository layout intent

- `services/*` is for runtime .NET services.
- `platform/*` is for cross-cutting backend capabilities used by multiple services.
- `packages/dotnet/*` is for reusable .NET packages (contracts, SDKs, primitives), not for service hosting.

## Central package and build policy

- `Directory.Packages.props` is the central NuGet package version governance point.
- `Directory.Build.props` defines repository-wide .NET build defaults.
- `Directory.Build.targets` defines shared build/test/analysis hooks.
- `global.json` pins SDK behavior for deterministic builds.

## Test placement

- Unit tests stay near the owning service or package.
- Cross-service integration tests belong in top-level `tests/*` with explicit scenario ownership.
- Performance and load tests should remain isolated from unit test pipelines.

## Migration placement

- Service-specific schema migrations stay inside the owning service boundary.
- Shared migration utilities can live in platform/package areas, but service data ownership must remain explicit.

## Service boundary rules

- Service runtime code can depend on `platform/*` and `packages/dotnet/*` where contracts allow.
- Service code must not depend on frontend runtime/app internals.
- Shared package dependencies should flow through stable contracts rather than direct service-to-service compile references.

## Frontend dependency prohibition

- .NET service and package code must not import or depend on frontend packages (`apps/*`, `packages/frontend/*`).
- Any cross-domain integration must happen via API/event contracts, not compile-time frontend coupling.
