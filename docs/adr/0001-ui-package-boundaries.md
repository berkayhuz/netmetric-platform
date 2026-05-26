# ADR 0001: UI Package Boundaries

- Status: Accepted
- Date: 2026-05-09

## Context

`@netmetric/ui` is shared across frontend apps. As app and component count grows, accidental internal imports and client/server boundary leaks become high-risk.

## Decision

- Public UI API is limited to explicit package exports.
- `@netmetric/ui` is server-safe/shared entry.
- `@netmetric/ui/client` is client-only entry.
- Internal files under `src/*` are not part of public contract unless exported in package `exports`.
- Boundary checks are enforced by lint and export smoke scripts.

## Consequences

- Safer API evolution and lower accidental breakage risk.
- Slightly stricter contribution process.
- Reduced chance of client code leaking into server entry points.
