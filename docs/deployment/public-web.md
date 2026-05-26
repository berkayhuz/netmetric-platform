# public-web deployment (netmetric.net)

This document defines the Kubernetes deployment wiring for `apps/public-web`, the public website for `https://netmetric.net`.

## Scope

- Serves public traffic for:
  - `https://netmetric.net`
  - `https://www.netmetric.net`
- Does not change `auth.netmetric.net` deployment behavior.
- Keeps `/` public (no login redirect).

## Manifests

Kubernetes resources are under:

- `deploy/kubernetes/public/public-web-serviceaccount.yaml`
- `deploy/kubernetes/public/public-web-configmap.yaml`
- `deploy/kubernetes/public/public-web-deployment.yaml`
- `deploy/kubernetes/public/public-web-service.yaml`
- `deploy/kubernetes/public/public-web-ingress.yaml`

## NEXT*PUBLIC*\* behavior (important)

`public-web` uses `NEXT_PUBLIC_*` values in both server and client code paths:

- Client-side usage is bundled/inlined at image build time.
- Server/runtime usage can read environment values at runtime.

Because of this, production URLs must be provided at **build time** (CI image build) and also set at **runtime** (Kubernetes env) for consistency.

Required values:

- `NEXT_PUBLIC_SITE_URL=https://netmetric.net`
- `NEXT_PUBLIC_AUTH_URL=https://auth.netmetric.net`
- `NEXT_PUBLIC_ACCOUNT_URL=https://account.netmetric.net`
- `NEXT_PUBLIC_CRM_URL=https://crm.netmetric.net`
- `NEXT_PUBLIC_TOOLS_URL=https://tools.netmetric.net`
- `NEXT_PUBLIC_API_URL=https://api.netmetric.net`

## Build-time requirement

When building the public-web container image, pass the same six values as build arguments or build environment values so the client bundle contains production hostnames.

`apps/public-web/Dockerfile` accepts these as build args and exports them as build-time env:

- `NEXT_PUBLIC_SITE_URL`
- `NEXT_PUBLIC_AUTH_URL`
- `NEXT_PUBLIC_ACCOUNT_URL`
- `NEXT_PUBLIC_CRM_URL`
- `NEXT_PUBLIC_TOOLS_URL`
- `NEXT_PUBLIC_API_URL`

## Runtime requirement

`public-web-configmap.yaml` defines the same six non-secret public URL values and injects them into the pod via `envFrom`.

## Image build and publish

GitHub Actions workflow:

- `.github/workflows/public-web-image.yml`

Behavior:

- Builds from `apps/public-web/Dockerfile`
- Pushes immutable image tag:
  - `ghcr.io/netmetric/public-web:${GITHUB_SHA}`
- Passes required `NEXT_PUBLIC_*` URLs as Docker build args

## Deployment image tag strategy

Do not deploy `latest`.

Before rollout, replace the placeholder image tag in `public-web-deployment.yaml` or patch in-cluster:

```bash
kubectl set image deployment/public-web public-web=ghcr.io/netmetric/public-web:<git-sha> -n netmetric
```

Equivalent YAML replacement is also acceptable as long as the final deployed tag is immutable.

## TLS and ingress

- Ingress is configured for `netmetric.net` and `www.netmetric.net`.
- TLS references `public-web-netmetric-tls`.
- cert-manager annotation assumes a cluster issuer named `letsencrypt-prod`.

If the cluster uses different issuer naming, update the ingress annotation accordingly.
This repository currently does not contain a pre-existing ingress baseline manifest set to confirm cluster-wide issuer/class convention; treat these values as cluster prerequisites to verify during rollout.

## Canonical host

Application canonical metadata points to `https://netmetric.net`.

`www -> apex` redirect is not enforced in the manifest to avoid ingress-controller-specific redirect assumptions. Configure redirect at ingress controller policy layer or DNS edge layer where supported.

## Health endpoints

Health routes:

- `/health/live`
- `/health/ready`

These are used by liveness and readiness probes and are intentionally not included in sitemap routes.
