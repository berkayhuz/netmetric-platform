# Quality Gates

## Mandatory gates

The repository enforces these minimum checks:

1. `pnpm run repo:validate`
2. `pnpm run frontend:lint`
3. `pnpm run frontend:typecheck`
4. `pnpm run repo:format:check`
5. `pnpm run frontend:build`
6. `pnpm run dotnet:restore`
7. `pnpm run dotnet:build`
8. `pnpm run dotnet:test`
9. `pnpm run deploy:check`

`pnpm run repo:check` executes the primary non-build gates in one command.

## Git hook gates

### pre-commit

- staged secret scan
- lint-staged checks (Prettier + ESLint for relevant files)
- selective package typecheck based on changed paths

### commit-msg

- Conventional Commit format validation

### pre-push

- workspace validation
- build verification

## CI gates

GitHub Actions pipeline:

1. Path filtering and changed-scope detection
2. Workspace validation
3. App scaffold validation matrix (for changed app scopes)
4. Package quality matrix (`lint`, `typecheck`, `build`, `test`, `coverage`) for changed packages only
5. Executable `dotnet`, `native`, and `deploy` gates when those domains are in scope
6. Security pipeline with Gitleaks + dependency vulnerability checks (high/critical fails)

All CI installs use:

- pnpm cache
- frozen lockfile
- corepack
- Node version pinned from `.nvmrc`

Execution is Turborepo-based for dependency-graph-aware scheduling and caching.

## Health checks

Run `pnpm run frontend:health` to execute:

- dead import detection
- unused export detection
- circular dependency detection
- dependency version consistency checks

## CI / Test / Quality hardening

- `docs/ci-test-quality-standards.md` documents P1-64 through P1-74 standards.
- Coverage gate command: `pnpm run coverage:threshold`
- Production config gate command: `pnpm run validate:prod-config`
- Image scan command: `pnpm run scan:images`
- Kubernetes manifest scan command: `pnpm run scan:k8s`
- E2E smoke command: `pnpm run test:e2e:smoke`
