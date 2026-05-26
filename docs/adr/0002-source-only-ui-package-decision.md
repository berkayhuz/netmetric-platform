# ADR 0002: Source-Only UI Package Decision

- Status: Accepted
- Date: 2026-05-09

## Context

`@netmetric/ui` currently exports source files directly. Dist/prebuilt output can be introduced later, but immediate risk is boundary drift rather than build mode.

## Decision

- Keep `@netmetric/ui` source-only in the current phase.
- Dist-build migration is explicitly deferred and is not P0 in this phase.
- Priority is boundary hardening: public API control, import restrictions, and smoke validation.

## Consequences

- No immediate build pipeline migration cost.
- Architectural safety improves before changing package build mode.
- Dist-build evaluation remains a separate future phase with dedicated rollout.
