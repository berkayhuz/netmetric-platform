# Workspace Command Guide

Root command strategy is namespace-based to keep orchestration clear and scalable.

Root script growth rule:

- root `package.json` remains a thin orchestration layer
- implementation details belong in domain tooling or `scripts/orchestration/*`
- app/service-specific runtime scripts must not be added at root
- new root scripts must stay inside these namespaces:
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

## `repo:*` commands

1. `pnpm run repo:validate`  
   Validates workspace structure, package naming integrity, and required repo files.

2. `pnpm run repo:format`  
   Formats repository files via Prettier.

3. `pnpm run repo:format:check`  
   Enforces deterministic formatting output.

4. `pnpm run repo:check`  
   Runs validation + peer integrity + frontend typecheck + frontend lint + format check.

5. `pnpm run repo:check:prod`  
   Runs deterministic install and full production gate including frontend build.

## `frontend:*` commands

1. `pnpm run frontend:lint`  
   Runs frontend lint tasks through Turborepo.

2. `pnpm run frontend:typecheck`  
   Runs strict TypeScript checks across frontend packages/apps.

3. `pnpm run frontend:build`  
   Verifies frontend packages/apps can build in current workspace state.

4. `pnpm run frontend:test`  
   Runs frontend test tasks through Turborepo graph orchestration.

5. `pnpm run frontend:coverage`  
   Runs coverage tasks for frontend packages that define coverage scripts.

6. `pnpm run frontend:check`  
   Runs frontend typecheck + lint + build + test.

7. `pnpm run frontend:health`  
   Runs dead import, unused export, circular dependency, dependency consistency, governance, and contract checks.

## Domain orchestration commands

1. `pnpm run dotnet:restore|dotnet:build|dotnet:test|dotnet:format|dotnet:check`  
   Orchestrates .NET domain operations when solution artifacts are present.

2. `pnpm run native:configure|native:build|native:test|native:check`  
   Orchestrates native domain operations when CMake components are present.

3. `pnpm run deploy:lint|deploy:validate|deploy:check`  
   Orchestrates deploy domain checks when deploy artifacts/tools are present.

## CI and guard commands

1. `pnpm run ci:frontend|ci:dotnet|ci:native|ci:deploy|ci:repo`  
   CI-oriented command groups.

2. `pnpm run guard:secrets`  
   Scans staged files for potential secret leaks.

3. `pnpm run guard:precommit`  
   Runs pre-commit sequence manually.

4. `pnpm run guard:prepush`  
   Runs pre-push sequence manually.

## CI quality and security commands

1. `pnpm run dotnet:coverage`  
   Produces deterministic .NET coverage artifacts for Sonar and threshold gating.

2. `pnpm run coverage:threshold`  
   Fails when frontend/.NET coverage reports are missing or below minimum thresholds.

3. `pnpm run scan:images`  
   Builds and scans all release images with Trivy (high/critical policy).

4. `pnpm run scan:k8s`  
   Runs kube-linter and Checkov against Kubernetes manifests.

5. `pnpm run validate:prod-config`  
   Validates production config safety contract (no localhost/unsafe fallbacks).

6. `pnpm run test:e2e:smoke`  
   Executes Playwright smoke flow when `RUN_E2E_SMOKE=1` is set.

## Contract and governance validation

1. `pnpm run contract:validate`  
   Enforces environment, exports, token/theme, API, and package naming contracts.

2. `pnpm run governance:validate`  
   Enforces frontend platform governance rules (naming, token policy, dark mode, motion, alias policy).

## Compatibility aliases

Backward-compatible aliases remain available:

- `check` -> `repo:check`
- `check:prod` -> `repo:check:prod`
- `lint` -> `frontend:lint`
- `typecheck` -> `frontend:typecheck`
- `build` -> `frontend:build`
- `test` -> `frontend:test`
- `coverage` -> `frontend:coverage`
- `workspace:validate` -> `repo:validate`
- `workspace:health` -> `frontend:health`
- `format` -> `repo:format`
- `format:check` -> `repo:format:check`
