# Changelog

All notable governance and package-level contract changes will be documented here.

## Unreleased

### Added

- Baseline governance docs for quality gates, TypeScript, token, performance, onboarding, and ADR index.
- UI boundary, export smoke, and test readiness hardening.

## Policy

### Breaking change communication

- Any public API removal/rename/type incompatibility must be listed under a **Breaking** section in the changelog.
- PR description must include migration notes and impacted import paths.

### Deprecation lifecycle

- Deprecate first, remove later.
- Deprecated APIs must include:
  - replacement API
  - deprecation phase/version note
  - planned removal phase/version note

### Support window

- Minimum expectation: one release phase/version overlap before removing deprecated public APIs.
- Emergency removals require explicit rationale in changelog and ADR/PR notes.
