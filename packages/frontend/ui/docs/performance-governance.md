# @netmetric/ui Performance & Bundle Governance

Related docs:

- [UI Public API Policy](README.md)
- [Token Governance](token-governance.md)
- [Quality Gates](/docs/quality-gates.md)

## Entry-point performance model

- `@netmetric/ui`:
  - server-safe/shared surface
  - preferred for core primitives and non-client-specific usage
- `@netmetric/ui/client`:
  - client-only surface
  - includes interactive/client-heavy modules and may pull larger runtime graphs

Use the smallest suitable entry for each call site. Importing from `client` by default increases client bundle exposure risk.

## Client-heavy component list

Representative client-heavy modules include:

- `Chart*` (`recharts`)
- `Carousel*` (`embla-carousel-react`)
- overlay cluster:
  - `Dialog`, `Sheet`, `Drawer`, `Tooltip`, `Popover`, `DropdownMenu`, `Menubar`, `Command`
- calendar stack:
  - `Calendar` (`react-day-picker`, `date-fns` transitively)

## Heavy dependency watchlist

- `recharts`
- `embla-carousel-react`
- `date-fns`
- Radix overlay cluster (`@radix-ui/react-dialog`, `popover`, `tooltip`, `dropdown-menu`, `menubar`, etc.)

When these dependencies are touched, run bundle smoke reporting and include size deltas in PR notes.

## CSS import policy

- `@netmetric/ui/styles/globals.css` should be imported once at app root/layout boundary.
- `theme.css` and `tokens.css` are public but should not be repeatedly imported in many leaf modules.
- Repeated style imports can increase duplicate CSS risk depending on bundler dedupe behavior.

## Why bundle budget is report-only now

- Current phase targets observability first.
- No hard fail threshold yet because baseline historical size variance is not established.
- False-positive fail gates are more expensive than temporary report-only visibility.

## Promotion criteria to fail-gate

Promote from report-only to fail-gate when:

- at least 2-3 sprints of stable bundle-smoke history exists
- scenario set is stable and representative
- acceptable size thresholds are agreed by maintainers
- CI environment shows deterministic results

## Bundle smoke scope (current)

- core: `Button` + `Input`
- overlay: `Tooltip`
- data-heavy: `ChartContainer` + `ChartTooltip`
- client-entry representative: selected imports from `@netmetric/ui/client`

This smoke is informational and does not fail on size.
