# public-web

`public-web` is the NetMetric public website for `netmetric.net`.
It provides enterprise-facing product, platform, and policy content while keeping `auth.netmetric.net` as the dedicated authentication app.

## Public routes

- `/`
- `/product`
- `/crm`
- `/tools`
- `/developers`
- `/security`
- `/pricing`
- `/about`
- `/contact`
- `/status`
- `/privacy`
- `/terms`
- `/cookies`

## Development

From the monorepo root:

```bash
pnpm --filter public-web dev
```

## Validation

Run from the monorepo root:

```bash
pnpm --filter public-web typecheck
pnpm --filter public-web lint
pnpm --filter public-web build
pnpm run repo:format:check
```

## Required public environment variables

`public-web` uses these variables for canonical URLs and external platform links:

- `NEXT_PUBLIC_SITE_URL`
- `NEXT_PUBLIC_AUTH_URL`
- `NEXT_PUBLIC_ACCOUNT_URL`
- `NEXT_PUBLIC_CRM_URL`
- `NEXT_PUBLIC_TOOLS_URL`
- `NEXT_PUBLIC_API_URL`

See `.env.example` for local development defaults.

## UI boundary rule

Shared UI primitives must come only from:

- `@netmetric/ui`
- `@netmetric/ui/client`

Do not define primitive UI components inside `apps/public-web`.
