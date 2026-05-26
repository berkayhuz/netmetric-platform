# CI / Test / Quality Standards (P1-64 .. P1-74)

## Test command matrix

- `pnpm --filter crm-web test`
- `pnpm --filter public-web test`
- `pnpm --filter tools-web test`
- `pnpm test`
- `pnpm --filter crm-web coverage`
- `pnpm --filter public-web coverage`
- `pnpm --filter tools-web coverage`
- `pnpm run dotnet:coverage`
- `pnpm run coverage:threshold`

## Coverage policy

- Frontend package-level minimum line coverage starts at `20%` for `crm-web`, `public-web`, and `tools-web`.
- .NET minimum line coverage starts at `20%` via `TestResults/dotnet/coverage-summary.json`.
- Missing report is a hard fail.
- Thresholds are intentionally incremental and can be raised after baseline stabilizes.

## Sonar import standard

- Frontend LCOV paths include `apps/crm-web`, `apps/public-web`, and `apps/tools-web`.
- .NET coverage generation runs before Sonar in CI (`pnpm run dotnet:coverage`).

## Security workflows

- CodeQL: `.github/workflows/codeql.yml` (C# + JS/TS, PR + main/release + weekly).
- Container scanning: `.github/workflows/image-scan.yml` with Trivy high/critical fail.
- Kubernetes scanning: `.github/workflows/k8s-manifest-scan.yml` with kube-linter + Checkov.

## Dependency automation policy

- Dependabot config: `.github/dependabot.yml`.
- npm and NuGet minor/patch updates are grouped.
- Major updates are kept separate.
- GitHub Actions and Docker base images are included.
- Auto-merge is disabled by default.

## E2E smoke policy

- Smoke entry point: `pnpm run test:e2e:smoke`.
- Default mode is skip unless `RUN_E2E_SMOKE=1`.
- CI PR path remains safe; full smoke is manual `workflow_dispatch` by default.

## Duplicate email regression scope

- Application handler unit test validates normalized case + whitespace duplicate rejection.
- API functional test validates `duplicate_email` contract without leaking user enumeration details.
- DB unique index integration tests protect cross-tenant duplicate normalized email constraints.

## Production config validation

- Script: `node ./scripts/release/validate-production-config.mjs`.
- Checks for missing production config files.
- Fails on unsafe localhost/example placeholders and dev/test fallbacks.
- Wired into `.github/workflows/dotnet-auth.yml` and release gate report.
