# mobile-app Placeholder

`apps/mobile-app` is an intentional placeholder and is not part of the active release pipeline.

## Current Status

- No mobile runtime scaffold is checked in.
- No package manifest is intentionally present.
- `.env.example` reserves future environment naming.

## Release Safety

- Build/lint/test commands must not fail when this folder remains placeholder-only.
- CI path filters can monitor the directory without forcing app build steps.

## Activation Criteria

If mobile scope is activated, add:

1. Mobile framework scaffold with deterministic build scripts.
2. Lint/test/smoke commands.
3. Environment matrix and deployment notes.
