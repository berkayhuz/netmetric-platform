# Kubernetes and Helm Assessment

## Current repository state

- `apps/*` directories are currently scaffolds without active Next.js runtime packages.
- Frontend production runtime deployment targets are not yet defined per app.
- Existing `deploy/helm` and `deploy/kubernetes` directories are present but contain no frontend manifests.

## Decision

At this phase, introducing Helm charts or Kubernetes manifests for frontend workloads is premature.

## Why it is premature

1. No concrete runtime app artifact exists yet (`apps/*` has no buildable Next.js app package).
2. Deployment contract is undefined (single tenant vs multi-tenant, CDN strategy, ingress model, edge runtime decisions).
3. Early manifests would become placeholders and create infrastructure drift.

## Minimal recommendation now

1. Keep deployment directories reserved but empty.
2. Define deployment prerequisites before adding infra:
   - app runtime ownership
   - container build strategy
   - environment promotion policy
   - secrets and config delivery mechanism
3. Add frontend deployment manifests only after at least one Next.js app reaches stable build and release cadence.
