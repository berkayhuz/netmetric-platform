# account-web deployment (account.netmetric.net)

This document defines Kubernetes deployment wiring for `apps/account-web`, served at `https://account.netmetric.net`.

## Scope

- Serves account portal traffic for `https://account.netmetric.net`.
- Does not change `auth.netmetric.net` deployment behavior.
- Uses existing account-web auth/session bridge.

## Manifests

Kubernetes resources are under:

- `deploy/kubernetes/account-web/account-web-serviceaccount.yaml`
- `deploy/kubernetes/account-web/account-web-configmap.yaml`
- `deploy/kubernetes/account-web/account-web-deployment.yaml`
- `deploy/kubernetes/account-web/account-web-service.yaml`
- `deploy/kubernetes/account-web/account-web-ingress.yaml`

## Build-time vs runtime env

`account-web` relies on both build-time and runtime environment values:

- Build-time values are required for `NEXT_PUBLIC_*` values used in client/server bundled code.
- Runtime values are required for server-side behavior and internal API routing.

### Build-time values (image build)

Use these Docker build args:

- `NEXT_PUBLIC_APP_NAME=NetMetric Account`
- `NEXT_PUBLIC_APP_ORIGIN=https://account.netmetric.net`
- `NEXT_PUBLIC_ACCOUNT_URL=https://account.netmetric.net`
- `NEXT_PUBLIC_AUTH_URL=https://auth.netmetric.net`
- `NEXT_PUBLIC_API_BASE_URL=https://api.netmetric.net`

### Runtime values (Kubernetes env)

Set via `account-web-configmap.yaml`:

- `NEXT_PUBLIC_APP_NAME`
- `NEXT_PUBLIC_APP_ORIGIN`
- `NEXT_PUBLIC_ACCOUNT_URL`
- `NEXT_PUBLIC_AUTH_URL`
- `NEXT_PUBLIC_API_BASE_URL`
- `ACCOUNT_API_BASE_URL=http://account-api` (server-only internal endpoint)
- `ACCOUNT_ACCESS_COOKIE_NAME=__Secure-netmetric-access`
- `NETMETRIC_COOKIE_DOMAIN=.netmetric.net` (recommended for production/staging shared locale cookie scope)

Do not publish `ACCOUNT_API_BASE_URL` as a public URL.

### `nm_locale` cookie scope

`account-web` sets `nm_locale` when users change language preferences. For cross-subdomain locale reuse (`account.netmetric.net` -> `auth.netmetric.net` / `crm.netmetric.net`), configure:

- `NETMETRIC_COOKIE_DOMAIN=.netmetric.net`

Expected production/staging cookie attributes:

- `secure=true`
- `sameSite=lax`
- `path=/`
- `domain=.netmetric.net`
- Do **not** use `__Host-` prefix for this cookie; `__Host-` cookies cannot include `Domain`.

Local development requirements:

- Leave `NETMETRIC_COOKIE_DOMAIN` empty/unset.
- Do not set a cookie `Domain` attribute on `localhost`, `127.0.0.1`, or `::1`.
- Local cookie behavior continues host-local without subdomain sharing.

## Image build and publish

GitHub Actions workflow:

- `.github/workflows/account-web-image.yml`

Behavior:

- Builds from `apps/account-web/Dockerfile`
- Pushes immutable image tag:
  - `ghcr.io/netmetric/account-web:${GITHUB_SHA}`
- Does not push `latest`

## Deployment image tag strategy

Do not deploy `latest`.

Replace placeholder in deployment or patch in-cluster:

```bash
kubectl set image deployment/account-web account-web=ghcr.io/netmetric/account-web:<git-sha> -n netmetric
```

## TLS and ingress

- Ingress host is `account.netmetric.net`.
- The TLS certificate reference is configured in `deploy/kubernetes/account-web/account-web-ingress.yaml`.
- The cert-manager issuer annotation is configured in the ingress manifest and must match the target cluster convention.
- The ingress class is configured in the ingress manifest and must match the target cluster convention.

These values follow the existing public-web pattern and should be verified against cluster-specific ingress and issuer conventions during rollout.

## Health endpoints

Health routes:

- `/health/live`
- `/health/ready`

These endpoints are unauthenticated and return minimal JSON suitable for Kubernetes probes.

## Auth/cookie assumptions

account-web bridge expects an auth access cookie readable by `account.netmetric.net`.

- If cookie strategy uses `__Secure-` with `Domain=.netmetric.net`, account-web can read it across subdomains.
- If cookie strategy uses `__Host-`, it cannot be shared across subdomains and account-web bridge will fail.

Also verify:

- auth return-url allowlist includes `https://account.netmetric.net`
- login at `auth.netmetric.net` can safely return to account-web

## DNS requirements

- DNS record for `account.netmetric.net` must target the ingress/load balancer endpoint for the netmetric cluster.

## Smoke checks

```bash
curl -i https://account.netmetric.net/health/live
curl -i https://account.netmetric.net/health/ready
```

Manual checks:

- Unauthenticated `https://account.netmetric.net` redirects to auth login with safe returnUrl
- Login returns to `https://account.netmetric.net`
- Protected routes load after login:
  - `/profile`
  - `/preferences`
  - `/security`
  - `/notifications`
  - `/settings`
  - `/settings/team`
- Run mutation smoke tests only with test accounts
