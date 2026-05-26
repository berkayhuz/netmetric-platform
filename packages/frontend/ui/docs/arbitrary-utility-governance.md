# @netmetric/ui Arbitrary Utility Governance

Related docs:

- [UI Public API Policy](README.md)
- [Token Governance](token-governance.md)

## Allowed patterns

- CSS-variable-based utilities:
  - `w-(--sidebar-width)`, `rounded-(--cell-radius)`, `size-(--cell-size)`
- Platform/runtime variable usage:
  - Radix CSS variables such as `var(--radix-select-trigger-width)`
- Controlled selector utilities for third-party DOM contracts:
  - Recharts and Radix selector overrides like `[&_.recharts-*]` where no cleaner API exists.
- State/data attribute selectors:
  - `data-[state=open]:...`, `group-data-[...]:...` when component state is attribute-driven.

## Avoid / disallow patterns

- Magic pixel values without token linkage:
  - avoid ad-hoc `top-[37px]`, `z-[9999]`, arbitrary spacing that is not variable-backed.
- Ad-hoc colors bypassing token system:
  - avoid raw hex/rgb/hsl inline utility colors unless required for third-party rendering edge cases.
- Repeated long selector chains copied across multiple files without abstraction.
- Unbounded arbitrary z-index values that bypass global z-scale.

## Complex component decision model

- Sidebar/calendar/chart are high-complexity style domains.
- Before adding arbitrary syntax:
  - check if existing token or semantic utility already solves the need
  - prefer CSS-variable-backed arbitrary value over hardcoded value
  - keep third-party selectors localized to the owning component
  - if reused across components, promote to domain token alias or shared class pattern

## Review checklist

- Is arbitrary value variable-backed?
- Is there an existing token alternative?
- Is this selector tied to third-party markup contract?
- Is the same pattern already duplicated elsewhere?
- Does it preserve dark/light parity?
