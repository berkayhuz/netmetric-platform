# P1 Auth, Account, and CRM Production Notes

Scope: ROADMAP `P1-16` through `P1-38` only.

## Auth and Account

### Register transaction strategy

The register path keeps the existing bounded model intact and performs user,
tenant, and owner membership creation inside the existing unit-of-work boundary.
The flow now has explicit regression coverage for consistent creation and keeps
the P0 duplicate-email protection in place. It does not remove the current
`User.TenantId` compatibility field, so the broader global-identity migration is
left open instead of being marked complete in the roadmap.

### Auth rate limiting

Sensitive auth endpoints use named rate-limit policies for login, register,
password recovery, and email-confirmation flows. Partition keys include the
policy, caller IP, tenant context, and endpoint path so test and development
windows can be configured independently without exposing account-enumeration
details in responses. Database-level uniqueness and handler-level checks remain
the final race-condition protection.

### MFA refresh-token behavior

Refresh-token renewal is covered by regression tests for the case where MFA is
enabled after a session was created. A pre-MFA refresh token cannot silently mint
a new access token after MFA becomes required; the session is treated according
to the security contract and revoked/rejected instead. Verified MFA and recovery
code flows remain separate.

### Account outbox and security notifications

Account security notifications now publish through the account outbox rather
than a production no-op. Password, MFA, session, profile, preferences, and avatar
events use versioned routing keys and safe payloads with tenant, user, session,
and correlation context where available. Payloads avoid sensitive secrets and
keep downstream notification services decoupled from the account transaction
handlers.

### Media cleanup and validation

Avatar replacement and deletion mark prior media assets as cleanup candidates.
The cleanup service only deletes assets that remain unreferenced after the grace
period, and storage-delete failures are logged for retry instead of breaking the
user-facing profile update. Upload validation now checks magic bytes, declared
MIME, image decode success, size, dimensions, metadata stripping, sanitized
encoding, and a scanner extension point. The development/test scanner can be
no-op; production configuration validation prevents an unsafe no-op scanner when
security scanning is required.

### Account sessions

Session revocation revalidates the account security routes so the session list
refreshes immediately after revoke/logout flows. Current-session handling stays
server-trusted, and last-seen values remain stored in UTC while UI formatting is
driven by locale, timezone, and date-format preferences.

## CRM

### Session and capability contract

CRM now loads a server-trusted session profile instead of trusting token shape
alone. The profile includes tenant, user, session, account status, email
confirmation status, and permission data. Frontend capabilities are derived from
that trusted profile and unknown capabilities default to deny. This capability
map is still UX-only; backend policies remain the enforcement boundary.

### Duplicate detection

Customer duplicate warnings are wired to the existing tenant-scoped duplicate
endpoint and rendered as a review-only warning on customer detail pages. Merge,
ignore, and create/update-time duplicate workflows are intentionally not marked
complete because the backend merge contract is not finished in this phase.

### Import/export failure strategy

The existing import/export work remains documented as not fully closed for
`P1-30`. The desired production contract is schema validation, preview/dry-run,
row-level error reporting, duplicate warnings, permission-checked export, audit
events for PII export, and an explicit all-or-batch rollback policy. This phase
does not claim that full contract as complete.

### CRM audit standard

The current persistence audit infrastructure remains in place, but `P1-31` is
not marked complete because a single masked audit contract for create, update,
delete, import, and export across all CRM modules still needs a dedicated pass.
The required production shape is user id, tenant id, entity type, entity id,
action, correlation id, timestamp, and a safe before/after summary.

### Public capture abuse protection

Anonymous lead capture now has endpoint-level rate limiting, request size
limits, origin/referer allowlist checks, honeypot rejection, and a configurable
captcha/challenge extension point. Responses remain generic to avoid leaking
tenant or account state. The item remains open until API regression tests cover
the new abuse-protection paths end to end.

### Signed marketing consent tokens

Anonymous unsubscribe and consent actions require signed, tenant-scoped,
purpose-scoped, expiring tokens with replay protection via a distributed-cache
marker. Invalid, expired, tampered, cross-tenant, or replayed tokens fail with a
safe generic response. Production requires a sufficiently long signing key.

### Webhook replay protection

Webhook endpoints have provider rate limits and request size limits. Existing
signature and idempotency handling is retained and production validation protects
provider configuration. The item remains open until timestamp and nonce replay
coverage is uniform across every provider and backed by explicit regression
tests.

### Mock integration provider

The mock integration provider is disabled in production by registry filtering,
and production configuration validation fails fast when the mock provider is
enabled. Development and test environments can keep the provider available for
local workflows.

### Module readiness

CRM module registry states are now treated as contract states: `ready`,
`source_visible`, `contract_pending`, and `disabled`. Routes and navigation only
activate modules that are backend-ready and allowed by tenant, plan, and
capability checks. Existing contract-pending disabled behavior is preserved.

### Date/time preferences

CRM date/time rendering goes through a shared helper that accepts locale,
timezone, and date-format preferences from account preference cookies/session
settings. Storage remains UTC. Invalid or missing preferences fall back safely so
server rendering and client hydration use the same formatted values.

### CRM error handling

CRM API errors now map through a shared helper for 401, 403, 404, 409, 422, and
500 behavior. Unauthorized sessions redirect through the existing session flow,
forbidden and not-found states render safe UX, conflict and validation responses
map to user-facing messages/field errors, and 500 responses are sanitized to
avoid leaking internal details.
