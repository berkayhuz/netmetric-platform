# Deployment Hardening (P1-52 .. P1-63)

## Frontend Docker standalone runtime strategy

- `account-web`, `crm-web`, `tools-web`, `public-web` Dockerfile files now use Next standalone runtime copy only (`.next/standalone`, `.next/static`, `public`).
- Runtime images run as non-root (`netmetric`) and do not copy the full repository.
- `auth-web` has no Dockerfile in current repo; deployment scope is unchanged for that app.

## Backend Dockerfile path/build strategy

- Account API path: `services/account/src/NetMetric.Account.API`.
- CRM API path: `services/crm/src/NetMetric.CRM.API`.
- Gateway path: `platform/gateway/src/NetMetric.ApiGateway`.
- Notification Worker path: `services/notification/src/NetMetric.Notification.Worker`.
- All runtime images run as non-root and expose only required runtime artifacts.

## Image publish workflow and tag strategy

- Added `.github/workflows/core-backend-images.yml` for Auth API, Account API, CRM API, Gateway, Notification Worker.
- Tags: git SHA always, semver on tag push, `latest` only for `main` and `release/*`.
- PR builds never push images.

## Kubernetes core backend manifest matrix

- Added `deploy/kubernetes/core/*` for `auth-api`, `account-api`, `crm-api`, `gateway`, `notification-worker` plus network policies.
- Each workload includes ServiceAccount, resources, probes (worker uses startup probe), security context, and PDB.

## Image tag automation and release gate

- Manifests use `${IMAGE_TAG}` instead of `REPLACE_WITH_GIT_SHA`.
- Added `scripts/release/validate-deployment-surface.mjs` and wired it into `scripts/release/release-gate.mjs`.

## Readiness/liveness health strategy

- Frontend readiness endpoints now fail with `503` when required env vars are missing.
- API liveness remains lightweight; readiness uses service-specific health checks.

## Tools API artifact storage strategy

- `deploy/kubernetes/tools/tools-api-deployment.yaml` now mounts PVC `tools-api-artifacts`.
- Added `deploy/kubernetes/tools/tools-api-pvc.yaml`.
- Production should not use ephemeral pod filesystem for artifact persistence.

## Port matrix

- Runtime container port standard is HTTP `8080` for backend APIs and gateway.
- Kubernetes Services target named `http` port and map to container port `8080`.
- Frontend workloads remain 7003/7004/7005/7006.

## Workload security baseline

- Deployment specs include `runAsNonRoot`, `allowPrivilegeEscalation: false`, `readOnlyRootFilesystem: true`, `capabilities.drop: ["ALL"]`, `seccompProfile: RuntimeDefault`.
- Added PodDisruptionBudget for all core backend workloads.
- Added default deny and scoped allow NetworkPolicy resources under `deploy/kubernetes/core/network-policies.yaml`.

## EF migration job release sequence

- Added migration job manifests in `deploy/kubernetes/core/migrations.yaml`.
- Use migration jobs before deployment rollout; app startup migrations remain disabled in production config.
- Rollback strategy: stop rollout, revert image tag, and run matching previous migration job only if migration is reversible.

## Production/staging deployment effects

- Production: stricter readiness, mandatory image tag injection, and volume-backed tools artifacts.
- Staging: same manifest shape with staging secrets/config values; `${IMAGE_TAG}` injection uses staging pipeline SHA/tag.
