# crm-web deployment (crm.netmetric.net)

This document defines deployment wiring for `apps/crm-web`, served at `https://crm.netmetric.net`.

## Scope

- Deploys the existing protected CRM frontend.
- Does not add new CRM modules.
- Keeps Leads/Opportunities/Pipeline/Tasks/Tags as contract-pending.

## Manifests

Kubernetes resources are under:

- `deploy/kubernetes/crm-web/crm-web-serviceaccount.yaml`
- `deploy/kubernetes/crm-web/crm-web-configmap.yaml`
- `deploy/kubernetes/crm-web/crm-web-deployment.yaml`
- `deploy/kubernetes/crm-web/crm-web-service.yaml`
- `deploy/kubernetes/crm-web/crm-web-ingress.yaml`

## Build-time vs runtime env

`crm-web` uses a split env model:

- `NEXT_PUBLIC_*` values are required at image build time for bundled client/server public config.
- `CRM_*` values are runtime server-only configuration.

### Build-time values (image build)

Pass as Docker build args:

- `NEXT_PUBLIC_APP_NAME=NetMetric CRM`
- `NEXT_PUBLIC_APP_ORIGIN=https://crm.netmetric.net`
- `NEXT_PUBLIC_CRM_URL=https://crm.netmetric.net`
- `NEXT_PUBLIC_AUTH_URL=https://auth.netmetric.net`
- `NEXT_PUBLIC_API_BASE_URL=https://api.netmetric.net`

### Runtime values (Kubernetes env)

Set from `crm-web-configmap.yaml`:

- `NEXT_PUBLIC_APP_NAME`
- `NEXT_PUBLIC_APP_ORIGIN`
- `NEXT_PUBLIC_CRM_URL`
- `NEXT_PUBLIC_AUTH_URL`
- `NEXT_PUBLIC_API_BASE_URL`
- `CRM_API_BASE_URL=http://api-gateway`
- `CRM_AUTH_SESSION_BASE_URL=http://api-gateway`
- `CRM_ACCESS_COOKIE_NAME=__Secure-netmetric-access`

Important:

- `CRM_API_BASE_URL` is server-only and must not be exposed as `NEXT_PUBLIC_*`.
- Server-side CRM API requests must use `CRM_API_BASE_URL`.
- Server-side auth session introspection/logout/shell hydration should use `CRM_AUTH_SESSION_BASE_URL`
  (defaults to `CRM_API_BASE_URL` when omitted).

## Image build and publish

GitHub Actions workflow:

- `.github/workflows/crm-web-image.yml`

Behavior:

- Builds from `apps/crm-web/Dockerfile`
- Pushes immutable image tag only:
  - `ghcr.io/netmetric/crm-web:${GITHUB_SHA}`
- Does not publish `latest`

## Immutable tag rollout

Do not deploy `latest`.

Patch deployment image with a specific commit SHA:

```bash
kubectl set image deployment/crm-web crm-web=ghcr.io/netmetric/crm-web:<git-sha> -n netmetric
```

## DNS and ingress

- DNS must route `crm.netmetric.net` to the cluster ingress endpoint.
- Ingress host is `crm.netmetric.net`.
- The ingress routes only to `crm-web` service.
- The TLS certificate reference is configured in the ingress manifest.
- Use the cluster-approved certificate reference for `crm.netmetric.net`.
- The ingress class and cert-manager issuer annotation follow existing frontend deployment conventions and should be verified in target cluster.

## Auth and cookie assumptions

crm-web depends on the existing auth session bridge model:

- Unauthenticated CRM routes redirect to auth login with safe returnUrl.
- auth returnUrl allowlist must include `https://crm.netmetric.net`.
- auth login must safely return to `https://crm.netmetric.net`.
- Health endpoints remain unauthenticated.

Cookie behavior assumptions to verify in staging:

- `__Host-` cookies cannot be shared across subdomains.
- If shared subdomain auth is required, use compatible cookie domain/flags strategy (for example `__Secure-` with valid domain, `Secure`, `HttpOnly`, and appropriate `SameSite` behavior).

## Health endpoints

crm-web health routes:

- `/health/live`
- `/health/ready`

These are minimal probe endpoints and should stay unauthenticated.

## Smoke checklist

- `https://crm.netmetric.net/health/live` returns success.
- `https://crm.netmetric.net/health/ready` returns success.
- Unauthenticated `https://crm.netmetric.net/dashboard` redirects to auth login with safe returnUrl.
- Auth login returns to `https://crm.netmetric.net`.
- `/dashboard` loads when authenticated.
- `/customers` list/detail/new/edit/delete flow works.
- `/companies` list/detail/new/edit/delete flow works.
- `/contacts` list/detail/new/edit/delete flow works.
- Customer/company address add/update/delete works.
- `/access-denied`, `/retry-later`, `/service-unavailable` render correctly.
- Browser storage check: no auth tokens in `localStorage`/`sessionStorage`.
- Rendered UI and error messages do not expose bearer/cookie values.
