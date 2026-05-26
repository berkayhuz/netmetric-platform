# admin-web Placeholder

`apps/admin-web` is an intentional placeholder and is not part of the active build/test matrix.

## Current Status

- No runtime scaffold is published.
- No package manifest is provided on purpose.
- `.env.example` exists only to reserve naming conventions.

## Release Safety

- Workspace scripts must not assume `admin-web` build artifacts.
- CI change detection may observe this folder, but app build jobs must remain unaffected.

## Activation Criteria

If admin-web becomes active scope, add:

1. `package.json` with lint/test/build scripts.
2. Minimal app scaffold and smoke test.
3. Environment documentation entry in `docs/operations/environment-matrix.md`.
