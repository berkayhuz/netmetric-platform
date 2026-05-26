# tools-web and tools-api deployment (tools.netmetric.net)

This document defines deployment wiring for the Tools surface:

- `apps/tools-web` served at `https://tools.netmetric.net`
- `services/tools/src/NetMetric.Tools.API` served internally as `tools-api`

## Scope

- Public host: `https://tools.netmetric.net`
- Internal Tools API service for server-side history/artifact calls from tools-web
- No server-side tool execution endpoint in this phase

## Kubernetes manifests

Resources are under `deploy/kubernetes/tools/`:

- `tools-web-serviceaccount.yaml`
- `tools-web-configmap.yaml`
- `tools-web-deployment.yaml`
- `tools-web-service.yaml`
- `tools-web-ingress.yaml`
- `tools-api-serviceaccount.yaml`
- `tools-api-configmap.yaml`
- `tools-api-deployment.yaml`
- `tools-api-service.yaml`

## Build-time vs runtime environment

### tools-web build-time public values

`apps/tools-web/Dockerfile` accepts these build args:

- `NEXT_PUBLIC_APP_NAME=NetMetric Tools`
- `NEXT_PUBLIC_APP_ORIGIN=https://tools.netmetric.net`
- `NEXT_PUBLIC_TOOLS_URL=https://tools.netmetric.net`
- `NEXT_PUBLIC_AUTH_URL=https://auth.netmetric.net`
- `NEXT_PUBLIC_API_BASE_URL=https://api.netmetric.net`

These values must be passed during image build so client-side bundles contain production hostnames.

### tools-web runtime values

`tools-web-configmap.yaml` provides runtime values:

- `NEXT_PUBLIC_APP_NAME`
- `NEXT_PUBLIC_APP_ORIGIN`
- `NEXT_PUBLIC_TOOLS_URL`
- `NEXT_PUBLIC_AUTH_URL`
- `NEXT_PUBLIC_API_BASE_URL`
- `TOOLS_API_BASE_URL=http://tools-api`
- `TOOLS_ACCESS_COOKIE_NAME=__Secure-netmetric-access`

`TOOLS_API_BASE_URL` is server-only and must not be exposed as client-only config.

### tools-api runtime values

`tools-api-configmap.yaml` provides non-secret runtime values:

- `ASPNETCORE_ENVIRONMENT=Production`
- `Tools__Authorization__RequireAuthenticatedUserForHistory=true`
- `Tools__ArtifactStorage__RootPath=/var/lib/netmetric/tools-artifacts`

`tools-api-deployment.yaml` also references `tools-api-secrets` for required secret values such as database/auth settings.

## Image build and push

GitHub Actions workflows:

- `.github/workflows/tools-web-image.yml`
- `.github/workflows/tools-api-image.yml`

Both workflows:

- support manual dispatch and push on `main` with path filters
- push immutable tags only:
  - `ghcr.io/netmetric/tools-web:${GITHUB_SHA}`
  - `ghcr.io/netmetric/tools-api:${GITHUB_SHA}`
- do not push `latest`

## Deploy immutable image tags

Patch deployments with immutable SHA tags:

```bash
kubectl set image deployment/tools-web tools-web=ghcr.io/netmetric/tools-web:<git-sha> -n netmetric
kubectl set image deployment/tools-api tools-api=ghcr.io/netmetric/tools-api:<git-sha> -n netmetric
```

## Ingress, DNS, and TLS

- `tools-web-ingress.yaml` maps host `tools.netmetric.net` to `tools-web` service.
- The TLS certificate reference is configured in the ingress manifest.
- The cert-manager issuer annotation is configured in the ingress manifest and must match the target cluster convention.
- DNS for `tools.netmetric.net` must point to the cluster ingress endpoint.

`tools-api` is intentionally internal-only in this phase (no public ingress manifest).

## Auth and cookie assumptions

- tools-web follows the account-web server-side bridge model for authenticated history/artifact actions.
- `__Host-` cookies cannot be shared across subdomains.
- Shared-subdomain login return flows require auth configuration to allow return to `https://tools.netmetric.net`.
- If auth cookie policy or return URL allowlist is missing, authenticated history features will fail even if deployment is healthy.

## Smoke checks

```bash
curl -i https://tools.netmetric.net/health/live
curl -i https://tools.netmetric.net/health/ready
```

Manual checks:

- `https://tools.netmetric.net` loads and public catalog/tool pages render.
- Guest QR and image conversion flows still run fully in-browser.
- Authenticated history pages work after login.
- tools-web can reach internal `tools-api` for authenticated history/artifact calls.
- `/history` is noindex and excluded from sitemap/robots indexing paths.
