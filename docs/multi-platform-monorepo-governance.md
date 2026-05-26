# Multi-Platform Monorepo Governance

This repository root is an orchestration layer for multiple technology domains. Domain runtime logic stays in domain folders, while root coordinates standards, quality gates, and CI strategy.

## Root: what it manages

Root manages:

- cross-repo standards (`.editorconfig`, `.gitattributes`, CODEOWNERS)
- orchestration tools (`package.json`, `pnpm-workspace.yaml`, `turbo.json`)
- CI workflows (`.github/workflows/*`)
- repo-level scripts (`scripts/*`)
- .NET global build policy (`Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `global.json`)

Root must stay thin:

- no product business logic
- no service-specific or app-specific runtime code
- no domain-specific script sprawl

## Frontend: what it manages

Frontend domain owns:

- `apps/*` for frontend app scopes
- `packages/frontend/*` for shared frontend platform packages
- frontend tests and frontend UI governance

Root should call frontend through namespaced orchestration scripts (`frontend:*`) instead of embedding package-level complexity.

## .NET services: what they manage

.NET domain owns:

- `services/*` for runtime Web API or worker services
- `platform/*` for cross-cutting backend capabilities
- `packages/dotnet/*` for reusable .NET packages only

`packages/dotnet/*` is used when code is reusable across multiple services/platform modules and should not host service runtime entrypoints.

## `platform/` folder purpose

`platform/` is for reusable backend capabilities (identity, security, observability, messaging, persistence abstractions) consumed by multiple services.

## `native/` folder purpose

`native/` is for C++ helpers, libraries, and DLLs where ABI-level or high-performance requirements exist. Native contracts must be explicit and testable.

## `tests/` folder split

Top-level `tests/` is reserved for cross-boundary test suites:

- integration tests
- e2e tests
- performance tests
- contract compatibility tests

Unit tests should remain close to their owning domain source.

## `deploy/` folder split

`deploy/` is for delivery artifacts only:

- `deploy/kubernetes`
- `deploy/helm`
- `deploy/terraform`

No application business logic should live in deploy files.

## `tools/` vs `scripts/`

- `scripts/`: orchestration commands that glue workflows together (CI helpers, validation runners).
- `tools/`: reusable developer tooling/codegen/analyzers that are themselves products/assets.

## Root `package.json`: how thin it should stay

Root scripts should be orchestration namespaces:

- `repo:*`
- `frontend:*`
- `dotnet:*`
- `native:*`
- `deploy:*`
- `ci:*`
- `guard:*`
- `security:*`
- `contract:*`
- `governance:*`

Root scripts should delegate to domain-owned tooling, not implement domain internals directly.

Root script growth guard:

- keep root `package.json` as a thin orchestration layer only
- place domain implementation scripts under domain folders or `scripts/orchestration/*`
- do not add app-specific or service-specific runtime scripts at root
- add new root scripts only if they match approved orchestration namespaces above

## SonarQube scope strategy

Current SonarQube configuration is intentionally frontend-only.

- active scope: frontend sources/tests and frontend coverage reports
- future scope: .NET and native Sonar expansion should be introduced only when domain-specific coverage/reporting pipelines are in place

Do not add placeholder .NET/C++ coverage paths before those pipelines exist.
