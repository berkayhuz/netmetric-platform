# Global Staging Smoke Gate

This document defines release/staging smoke validation for:

- `auth.netmetric.net`
- `netmetric.net` (`public-web`)
- `account.netmetric.net` (`account-web`)
- `tools.netmetric.net` (`tools-web`)
- `crm.netmetric.net` (`crm-web`)
- internal `tools-api` service
- API Gateway assumptions used by frontend apps

## A) Scope

This gate is for deployment readiness and environment validation only.

- No product feature changes.
- No auth/session behavior changes.
- No backend contract changes.
- No secret values in repo scripts/docs.

## B) Environment Levels

- Local development
- Staging
- Production

## C) Repo-Ready Assets

Already prepared in repository:

- Local frontend apps and scripts
- Dockerfiles for deployable surfaces
- GHCR image workflows
- Kubernetes manifests
- Per-app deployment documentation

## D) What Is Not Automatically Available In Development

Local development does not automatically provide:

- production DNS records
- production TLS certificates
- Kubernetes cluster/context
- cert-manager issuer availability
- production runtime secret references
- ingress/load-balancer routing

## E) Local Development Smoke

Expected local URLs:

- auth-web: `http://localhost:7002`
- public-web: `http://localhost:7003`
- account-web: `http://localhost:7004`
- tools-web: `http://localhost:7005`
- crm-web: `http://localhost:7006`

Primary command:

```powershell
pnpm frontend:start
```

Expected checks:

- public-web loads
- account-web protected flows redirect safely
- tools-web public tools load locally
- crm-web protected shell/auth redirect works
- health endpoints respond where implemented

Use:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\release\Test-LocalFrontend.ps1
```

Release-gate enforcement note:

- `scripts/release/release-gate.ps1` now treats Auth+Account smoke as required for release-scope runs.
- If local smoke is required and not executed (`--run-dev-smoke` missing), release gate fails.

## F) Validation Command Set

Frontend/workspace:

```powershell
pnpm frontend:typecheck
pnpm frontend:lint
pnpm frontend:build
pnpm run repo:validate
pnpm run repo:format:check
```

Backend guidance (when backend is in release scope):

```powershell
pnpm run dotnet:restore
pnpm run dotnet:build
pnpm run dotnet:test
```

## G) Docker Build Checks

Run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\release\Test-DockerImages.ps1
```

Build targets:

- `apps/public-web/Dockerfile`
- `apps/account-web/Dockerfile`
- `apps/tools-web/Dockerfile`
- `services/tools/src/NetMetric.Tools.API/Dockerfile`
- `apps/crm-web/Dockerfile`

Only safe/public build args are used. No secrets are passed.

## H) GitHub Actions / GHCR Checks

Image workflows:

- `.github/workflows/public-web-image.yml`
- `.github/workflows/account-web-image.yml`
- `.github/workflows/tools-web-image.yml`
- `.github/workflows/tools-api-image.yml`
- `.github/workflows/crm-web-image.yml`

Checkpoints:

- workflow run succeeds
- GHCR image exists
- immutable commit-SHA tag exists
- no `latest` tag dependency in rollout process

## I) Kubernetes Checks

Context/cluster readiness:

```powershell
kubectl config current-context
kubectl get nodes
kubectl get ns
```

Manifest validation:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\release\Test-KubernetesManifests.ps1 -DryRunOnly
```

Optional apply:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\release\Test-KubernetesManifests.ps1 -Apply -ConfirmApply
```

Rollout checks:

```powershell
kubectl rollout status deploy/public-web -n netmetric
kubectl rollout status deploy/account-web -n netmetric
kubectl rollout status deploy/tools-web -n netmetric
kubectl rollout status deploy/tools-api -n netmetric
kubectl rollout status deploy/crm-web -n netmetric
```

Debugging:

```powershell
kubectl get pods -n netmetric
kubectl get svc -n netmetric
kubectl get ingress -n netmetric
kubectl describe deploy <name> -n netmetric
kubectl logs deploy/<name> -n netmetric --tail=200
```

## J) Required Runtime Secret/Config References

Required references must exist in target namespace before production rollout.

- tools-api deployment references a required runtime secret resource named `tools-api-secrets`.
- Auth/account/backend services may require additional runtime secret references depending on their manifests and environment settings.
- The TLS certificate reference is configured in ingress manifests.
- cert-manager cluster issuer reference used by ingress annotations must exist in the cluster.
- Registry pull credential configuration is required only if cluster policy does not allow anonymous GHCR pulls for required images.

No secret values are stored in this repository.

## K) DNS / TLS / Ingress Checks

```powershell
kubectl get ingress -n netmetric
kubectl describe ingress public-web -n netmetric
kubectl describe ingress account-web -n netmetric
kubectl describe ingress tools-web -n netmetric
kubectl describe ingress crm-web -n netmetric
kubectl get certificate -A
kubectl get challenges -A
kubectl get orders -A
```

DNS and endpoint checks:

```powershell
nslookup netmetric.net
nslookup account.netmetric.net
nslookup tools.netmetric.net
nslookup crm.netmetric.net
nslookup auth.netmetric.net
curl -I https://netmetric.net
curl -I https://account.netmetric.net
curl -I https://tools.netmetric.net
curl -I https://crm.netmetric.net
curl -I https://auth.netmetric.net
```

## L) Auth Cookie / ReturnUrl Smoke

Validate:

- login at `auth.netmetric.net` can return safely to account/tools/crm hosts
- returnUrl allowlist includes:
  - `https://account.netmetric.net`
  - `https://tools.netmetric.net`
  - `https://crm.netmetric.net`
- `__Host-` cookies are not assumed to be shared across subdomains
- if bridge requires shared cross-subdomain session, verify compatible `__Secure-` domain/flags strategy in staging
- `HttpOnly`, `Secure`, and `SameSite` behavior supports intended auth redirects and protected route access

## M) Domain Smoke (Staging/Production)

Public:

- `https://netmetric.net`
- `https://netmetric.net/health/live`
- `https://netmetric.net/health/ready`
- `https://netmetric.net/robots.txt`
- `https://netmetric.net/sitemap.xml`

Account:

- `https://account.netmetric.net/health/live`
- `https://account.netmetric.net/health/ready`
- unauthenticated redirect behavior
- authenticated routes:
  - `/profile`
  - `/preferences`
  - `/security`
  - `/notifications`
  - `/settings`
  - `/settings/team`

Tools:

- `https://tools.netmetric.net/health/live`
- `https://tools.netmetric.net/health/ready`
- QR generator
- PNG/JPG conversion tools
- authenticated history save/download/delete

CRM:

- `https://crm.netmetric.net/health/live`
- `https://crm.netmetric.net/health/ready`
- unauthenticated redirect behavior
- dashboard
- customers/companies/contacts list/detail/new/edit/delete
- customer/company address add/update/delete

## N) Final GO / NO-GO Criteria

### GO: Repo Readiness

- All lint/type/build/format/validation gates pass.
- Docker image builds succeed locally or in CI.
- Required workflows exist and use immutable image tags.

### GO: Staging

- Kubernetes context reachable.
- Manifest dry-runs pass.
- Required runtime secret/config references exist.
- Ingress, DNS, and TLS checks pass.
- Auth returnUrl/cookie bridge checks pass for protected apps.
- Health endpoints pass for all deployed surfaces.

### GO: Production

- Staging GO achieved.
- Production image tags resolved to immutable commit SHA tags.
- Production namespace has required runtime references.
- Production domain smoke checks pass with no auth/session regressions.

### NO-GO Examples

- No usable Kubernetes context.
- Manifest dry-run/apply fails.
- Required image missing from GHCR.
- DNS does not resolve correctly.
- TLS/cert issuance not ready.
- Auth cookie bridge fails across required subdomains.
- returnUrl policy blocks required protected app redirect.
- Protected app leaks token/cookie data to client-visible output.
- Required health endpoint fails.
