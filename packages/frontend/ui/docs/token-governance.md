# @netmetric/ui Token Governance

Related docs:

- [UI Public API Policy](README.md)
- [Arbitrary Utility Governance](arbitrary-utility-governance.md)
- [Performance Governance](performance-governance.md)

## Token layers

- Foundation token:
  - Raw, reusable design primitives.
  - Example: spacing steps, base shadows, z-index scale, motion durations.
  - In this package, foundation tokens use `--nm-*`.
- Semantic token:
  - Meaning-driven alias used by components and utilities.
  - Example: `--color-background`, `--color-border`, `--color-focus-ring`.
  - Semantic tokens map foundation tokens through `theme.css`.
- Component/domain token:
  - Tokens scoped to a UI domain/component family.
  - Example: sidebar domain tokens (`--nm-sidebar-*`), chart color CSS vars.

## Token ownership

- `packages/frontend/ui/src/styles/tokens.css`:
  - source of truth for foundation tokens and domain aliases.
- `packages/frontend/ui/src/styles/theme.css`:
  - semantic mapping for Tailwind token consumption.
- Component-local CSS variables:
  - allowed only for contextual values that cannot be represented safely in global semantic tokens.

## Naming convention

- Foundation: `--nm-<category>-<name>`
  - examples: `--nm-space-4`, `--nm-shadow-md`, `--nm-z-modal`
- Semantic: `--color-*`, `--spacing-*`, `--text-*`, `--shadow-*`, `--z-*`, `--radius-*`
- Domain aliases:
  - `--nm-sidebar-*` for sidebar
  - `--nm-data-viz-*` for chart/data-viz shared aliases

## Token deprecation policy

- Do not delete active tokens directly.
- Mark deprecated tokens in docs with:
  - replacement token
  - migration window
  - planned removal phase
- Keep deprecated token aliases pointing to replacement until migration completes.

## Dark/light parity checklist

- Background/foreground pairs are defined for both light and dark.
- Border/input/ring/focus-ring are defined for both themes.
- Destructive and muted contrast is reviewed in both themes.
- Sidebar domain tokens have light/dark parity.
- Overlay/backdrop tokens exist and are not hardcoded per component when avoidable.
- Focus and disabled states remain visible in both themes.

## New token checklist

- Belongs to correct layer (foundation vs semantic vs domain).
- Reuses existing values where possible.
- Added to both light and dark theme when color-like.
- Mapped in `theme.css` if meant for Tailwind semantic use.
- Has clear consumer scope (global vs domain).
- Does not duplicate an existing token with different name.

## Token inventory (current)

- Color:
  - `--nm-background`, `--nm-foreground`, `--nm-primary`, `--nm-secondary`, `--nm-muted`, `--nm-accent`, `--nm-destructive`
- Surface:
  - `--nm-card`, `--nm-popover`, `--nm-sidebar`, `--nm-overlay-backdrop`
- Text:
  - `--nm-foreground`, `--nm-muted-foreground`, `--nm-sidebar-foreground`
- Border:
  - `--nm-border`, `--nm-input`, `--nm-sidebar-border`
- Spacing:
  - `--nm-space-1` to `--nm-space-6`
- Typography:
  - Size: `--nm-font-size-xs` to `--nm-font-size-xl`
  - Line-height aliases: `--nm-line-height-tight`, `--nm-line-height-normal`, `--nm-line-height-relaxed`
  - Weight aliases: `--nm-font-weight-normal`, `--nm-font-weight-medium`, `--nm-font-weight-semibold`
  - Letter-spacing aliases: `--nm-letter-spacing-tight`, `--nm-letter-spacing-normal`, `--nm-letter-spacing-wide`
- Radius:
  - `--nm-radius`
- Shadow/elevation:
  - `--nm-shadow-xs`, `--nm-shadow-sm`, `--nm-shadow-md`, `--nm-shadow-lg`
- Motion:
  - `--nm-motion-fast`, `--nm-motion-base`, `--nm-motion-slow`, `--nm-ease-standard`
- Focus:
  - `--nm-ring`, `--nm-focus-ring`
- Z-index:
  - `--nm-z-sticky`, `--nm-z-dropdown`, `--nm-z-overlay`, `--nm-z-modal`, `--nm-z-toast`
- Overlay:
  - `--nm-overlay-backdrop`
- Domain:
  - Sidebar: `--nm-sidebar-*`
  - Data-viz aliases: `--nm-data-viz-grid`, `--nm-data-viz-axis`, `--nm-data-viz-tooltip`
