# TypeScript Governance

## Core policy

- Strict mode is mandatory.
- Shared package contracts must remain explicit and stable.
- Public API changes require migration notes.

## Package responsibilities

1. `@netmetric/types`  
   Shared business and domain types across workspace modules.

2. `@netmetric/config`  
   Shared typed configuration and environment schema parsing.

3. `@netmetric/ui`  
   UI components and UI-facing types with controlled exports.

## Import boundaries

- Allowed public imports from UI package:
  - `@netmetric/ui`
  - `@netmetric/ui/client`
  - `@netmetric/ui/styles/*`
- Disallowed:
  - `@netmetric/ui/src/*`

## Compiler baseline

Frontend TS presets enforce:

- strict mode
- no unchecked indexed access
- exact optional property types
- force consistent casing

## Validation policy

- package-level `typecheck` scripts are required
- root `pnpm run frontend:typecheck` is the default workspace gate (`pnpm run typecheck` alias remains available)
- changed shared type contracts must be validated before merge
