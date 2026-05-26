# @netmetric/ui Public API Policy

## Related governance docs

- [Root README](/README.md)
- [Contributing](/docs/CONTRIBUTING.md)
- [Quality Gates](/docs/quality-gates.md)
- [TypeScript Governance](/docs/typescript-governance.md)
- [ADR Index](/docs/adr/README.md)
- `token-governance.md`
- `arbitrary-utility-governance.md`
- `performance-governance.md`
- `data-grid-v1.md`

## Package intent

- `@netmetric/ui`: server-safe and shared UI surface. Import tokens, styles, and server-compatible components from here.
- `@netmetric/ui/client`: client-only UI surface. Import hooks, interactive primitives, and components that require `"use client"` from here.

## Export boundary

### `@netmetric/ui` root entrypoint

Use root entrypoint for:

- server-safe components
- stateless/shared primitives
- non-interactive layout/display components
- shared helpers and types

### `@netmetric/ui/client` entrypoint

Use client entrypoint for:

- interactive components
- hooks
- browser-dependent components
- table runtime components
- shell/search orchestration

### Duplicate export compatibility list

The following exports are intentionally available from both root and client as compatibility surface:

- `CompactActionGroup`
- `NativeSelect`
- `EntityTableInfoStrip`
- `PageShell`
- `WorkspacePageShell`

## Boundary policy

- Do not add new duplicate exports.
- Do not add new interactive components to root entrypoint (`src/index.ts`).
- App/domain-heavy shell/search presets should be evaluated for a future app-kit layer.
- Existing duplicate exports remain compatibility-only and should not be removed in a breaking way.

## Table direction note

- New table work should use `DataTable`/`EntityDataTable`.
- `DataGrid` remains a legacy/deprecated candidate until removed after usage verification.

## Allowed imports

- App code may import only:
  - `@netmetric/ui`
  - `@netmetric/ui/client`
  - `@netmetric/ui/styles/globals.css`
  - `@netmetric/ui/styles/theme.css`
  - `@netmetric/ui/styles/tokens.css`
- Do not import `@netmetric/ui/src/*` or any file path under `packages/frontend/ui/src/*`.

## Styles usage

- Import `@netmetric/ui/styles/globals.css` once at app root layout level.
- `theme.css` and `tokens.css` are public only for explicit theme/token composition cases.
- Prefer consuming semantic tokens via Tailwind utilities and shared classes, not direct ad-hoc variable overrides.

## Client boundary rule

- Client components must not be exported from `src/index.ts`.
- Client components must be exported from `src/client.ts`.
- Why: keeping server and client entry points separate prevents accidental client bundle growth and server-component boundary leaks.

## Internal API rule

- `src/lib/*`, `src/hooks/*`, and non-exported component helpers are internal by default.
- Internal modules can be promoted only by adding explicit public export policy notes and release notes.

## Breaking change policy

- Any public export removal, rename, type-shape change, or behavior contract change is breaking.
- Breaking changes must be grouped in dedicated PRs with migration notes and semver-impact label.
- Large boundary refactors are not allowed in a single mixed-feature PR.

## Deprecation policy

- Deprecate first, remove later.
- Mark deprecated exports in docs and changelog/PR notes with:
  - replacement API
  - deprecation release tag/date
  - planned removal window
- Removal happens only after at least one planned migration cycle.

## New component checklist

- Component taxonomy placement is clear (`primitives`, `forms`, `overlay`, etc.).
- Server vs client boundary is explicit.
- Export target is explicit (`index.ts` or `client.ts`, never both unless intentionally split).
- Public name and variant API are stable and documented.
- Accessibility expectations are documented.
- Token usage follows semantic variables; no ungoverned arbitrary styles.
- Export smoke and lint checks pass.
