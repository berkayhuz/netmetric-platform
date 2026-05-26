# Native Layer Governance

`native/` hosts C++ helper libraries and DLL projects when platform-level performance or ABI-level integration is required.

## When to add code under `native/`

- A backend/frontend integration needs low-level performance that managed/runtime code cannot provide.
- A shared algorithm must be exposed as a stable binary contract to .NET or Node consumers.
- A third-party native dependency must be wrapped behind repository-owned contracts.

## CMake standard

- Each native component must be buildable through CMake.
- Root `native/` should remain orchestration-oriented; component-specific build logic belongs in component directories.
- Generator-specific files and build outputs must stay outside source folders.

## ABI contract rules

- Public ABI must be explicit and versioned.
- Breaking ABI changes require contract note and migration plan.
- Exported symbol visibility must be intentionally controlled.

## Generated bindings rule

- Generated interop bindings (for .NET P/Invoke or Node N-API wrappers) must be reproducible from source.
- Generated artifacts should be recreated by scripts/tooling, not manually edited.

## Sanitizer and test expectations

- Native components must provide unit tests.
- CI-ready sanitizer profiles (ASan/UBSan minimum where supported) should be defined per component.
- Crash repro and minimal diagnostic steps must be documented for each component.

## Exposure model to .NET and Node

- .NET integration should use explicit interop boundary packages (for example in `packages/dotnet/*`).
- Node integration should use dedicated binding layers, not direct runtime-dependent hacks in app code.
- Consumer layers must depend on stable contracts, not internal build artifacts.

## Binary artifacts policy

- Compiled binaries and intermediate outputs must not be committed.
- `.gitignore` and CI artifact flows should handle binary distribution and retention.
