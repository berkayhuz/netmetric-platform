# Domain and Cookie Strategy

## Domain Plan

- `netmetric.net` public website
- `auth.netmetric.net` identity flows
- `account.netmetric.net` user profile/preferences
- `crm.netmetric.net` CRM frontend
- `tools.netmetric.net` tools frontend
- `api.netmetric.net` gateway/API edge

## Cookie Classes

- `__Host-` cookies: host-only, path `/`, `Secure`, no `Domain`.
- `__Secure-` cookies: `Secure`, domain-scoped when cross-subdomain access is required.
- Parent-domain preference cookies: `.netmetric.net` for theme/locale sync.

## Auth and Session

- Auth/session cookies stay host-scoped where possible.
- Cross-app identity propagation is token/session-introspection based, not broad cookie sharing.
- Account and CRM must validate tenant membership on every protected request.

## Theme/Locale Sync

- Theme/locale preference cookie uses parent domain for auth/account/crm/tools continuity.
- Client-side localStorage may cache, but server cookie is source of truth.

## Security Flags

- Production: `Secure`, `HttpOnly` for session/auth cookies, `SameSite=Lax` or stricter per flow.
- Localhost development: relaxed domain handling without parent-domain override.

## Local Development

- Use `localhost` origins and development-safe cookie names.
- Do not reuse production cookie domain or signing keys.

## Consistency Rules

- Env matrix and cookie strategy must not diverge.
- Any new app/subdomain must register cookie behavior before release.
