# Environment Setup

## Environment tiers

Supported tiers:

- `development`
- `test`
- `staging`
- `production`

`APP_ENV` controls tier-specific behavior, while `NODE_ENV` remains Node runtime mode.

## Templates

Environment templates are committed as examples:

- root: `.env.example`
- app scopes: `apps/*/.env.example`

Do not commit real secret values.

## Required variables

Minimum required frontend variables:

- `NODE_ENV`
- `APP_ENV`
- `NEXT_PUBLIC_APP_NAME`
- `NEXT_PUBLIC_API_GATEWAY_BASE_URL` (preferred) or `NEXT_PUBLIC_API_BASE_URL` (legacy fallback)

Optional monitoring:

- `SENTRY_DSN`
- `NEXT_PUBLIC_SENTRY_DSN`

## Typed validation

Use `parseFrontendEnv` from `@netmetric/config`:

```ts
import { parseFrontendEnv } from "@netmetric/config/env";

const env = parseFrontendEnv(process.env);
```

Validation fails fast at startup when required variables are missing or malformed.

## Secret protection

Secret leakage prevention is enforced by pre-commit hooks:

- staged content is scanned for common secret signatures
- `.env.example` files are excluded from failure checks
- CI includes Gitleaks scanning with `.gitleaks.toml` allowlists for approved template/docs paths

If a false positive occurs, replace sensitive-like values with placeholders such as `example` or `changeme`.
