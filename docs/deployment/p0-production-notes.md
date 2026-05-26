# P0 Production Notes

This note records the production and staging effects of the P0-01 through P0-15 hardening pass.

## Identity and Auth

- Auth now treats `NormalizedEmail` as a global identity key. The migration fails fast when duplicates already exist; run the duplicate remediation plan before applying the unique index in production.
- Production login and refresh-token issuance require confirmed email when `AccountLifecycle.RequireConfirmedEmailForLogin` is enabled. Development and test environments can keep policy-controlled behavior.
- Auth return URLs are restricted to NetMetric production origins in production. Localhost remains a development-only allowance.
- Anonymous Auth gateway flows such as register, login, forgot/reset password, confirmation, refresh, logout, and session status do not require tenant forwarding. Tenant-specific endpoints keep tenant enforcement.

## Frontend Preferences

- Theme values use the `light`, `dark`, `system` contract across backend, DTOs, cookies, and client cache. Legacy `Default` input is normalized to `system`.
- The theme cookie is the server-render source of truth. Local storage is a client cache/fallback and must not override a newer cookie value.
- Production cookie sharing uses `NEXT_PUBLIC_NETMETRIC_COOKIE_DOMAIN`, with `.netmetric.net` configured in Kubernetes for account/crm/tools/public frontends. Local development omits the cookie domain.

## CRM Access

- CRM protected routes validate the access token through the gateway Auth session-status endpoint before rendering protected content.
- CRM route/layout guard runs centrally and checks route-level capabilities for read/create/edit access.
- CRM navigation and visible create/edit/delete/manage actions are derived from backend permission claims. Unknown permissions deny by default.
- `contract_pending` and `coming_soon` CRM modules are non-navigable; ready/read-only modules continue to work when capabilities allow them.
- Global Search and Quick Create stay disabled until their implementations are wired.

## Gateway and Cross-Service Config

- Production CORS allowlists only the explicit NetMetric origins: apex, www, auth, account, crm, tools, and api.
- Tools API traffic is routed through the gateway at `/api/v1/tools/*`, with direct service access reserved for internal service configuration.
- CRM server-side session introspection uses the gateway API base URL, not the direct CRM API base URL.
