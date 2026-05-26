# Contributing Guide

## Scope

This repository is managed as a multi-platform monorepo. Frontend packages and apps live in:

- `packages/frontend/*`
- `apps/*`

Additional domain roots:

- `services/*` for runtime .NET services
- `platform/*` for cross-cutting backend capabilities
- `packages/dotnet/*` for reusable .NET packages
- `native/*` for native helper libraries
- `deploy/*` for deployment artifacts

Do not mix architecture changes and product feature work in the same pull request.

## Prerequisites

- Node.js version from `.nvmrc`
- `pnpm` (version pinned in root `package.json`)

## First-time setup

1. `pnpm install`
2. `pnpm run repo:validate`
3. `pnpm run repo:check`

## Branch and commit policy

- Use short-lived branches per concern.
- Commit messages must pass Conventional Commit validation.
- Hooks run automatically:
  - `pre-commit`: secret scan, lint-staged, selective typecheck
  - `pre-push`: optional local gitleaks (if installed), workspace validation and build

## Quality gates

Minimum standards before opening a PR:

1. `pnpm run frontend:lint`
2. `pnpm run frontend:typecheck`
3. `pnpm run frontend:build`
4. `pnpm run frontend:test`
5. `pnpm run frontend:coverage`
6. `pnpm run repo:format:check`

Compatibility aliases (`lint`, `typecheck`, `build`, `test`, `coverage`, `format:check`) remain available during transition.

## Environment policy

- Do not commit real secrets.
- Keep only template values in `.env.example` files.
- Validate runtime env with `@netmetric/config` (`parseFrontendEnv`).

## Ownership model

Code ownership is enforced by `.github/CODEOWNERS` for:

- root governance
- frontend platform and apps
- .NET services and platform
- native systems
- deploy and infra
- tests and devex tooling

## Documentation policy

Before merging architecture changes, update related docs in `docs/`:

- `frontend-architecture.md`
- `environment-setup.md`
- `local-development.md`
- `workspace-commands.md`
- `frontend-platform-governance.md`
