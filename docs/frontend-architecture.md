# Frontend Architecture

## Foundation

The frontend platform is organized as a pnpm workspace monorepo:

- `apps/*`: runtime applications (Next.js app slots, currently scaffolded)
- `packages/frontend/*`: shared frontend platform packages

## Shared package boundaries

Current shared packages:

- `@netmetric/ui`: shadcn/ui-based component system and style surface
- `@netmetric/types`: cross-package TypeScript contracts
- `@netmetric/config`: typed frontend config and environment parsing
- `@netmetric/eslint-config`: reusable lint profiles
- `@netmetric/tailwind-config`: shared Tailwind CSS entry files
- `@netmetric/tsconfig`: strict TypeScript base presets

## Dependency flow

Rules for maintainability:

1. Apps can depend on shared packages.
2. Shared packages must not depend on apps.
3. UI package internal files must not be imported through `@netmetric/ui/src/*`.
4. Public UI API is limited to package exports (`@netmetric/ui`, `@netmetric/ui/client`, `@netmetric/ui/styles/*`).

## Runtime model

- All packages run in strict TypeScript mode.
- UI package keeps source exports and validates API boundaries with smoke checks.
- Environment values are validated via `parseFrontendEnv` from `@netmetric/config`.

## Quality model

Quality gates:

- lint
- typecheck
- build
- format check
- workspace validation

Health checks:

- dead imports (`lint`)
- unused exports (`knip`)
- circular dependencies (`madge`)
- dependency version consistency (`pnpm dedupe --check`)

## CI model

GitHub Actions pipeline:

1. change detection (path filtering + changed package/app matrix)
2. workspace validation (blocking)
3. package quality jobs orchestrated with Turborepo graph + cache
4. app scaffold validation for changed app scopes

The pipeline uses frozen lockfile installs and pnpm caching.
