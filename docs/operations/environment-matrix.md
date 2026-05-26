# Environment Matrix

## Legend

- Required: app/service startup fails if missing.
- Optional: feature-level fallback exists.
- Source: `.env`, Kubernetes Secret, Kubernetes ConfigMap, or external secret manager.

## Frontend Apps

| App         | Required                                                                                  | Optional                                         | Notes                                                                 |
| ----------- | ----------------------------------------------------------------------------------------- | ------------------------------------------------ | --------------------------------------------------------------------- |
| auth-web    | `NEXT_PUBLIC_APP_NAME`, `NEXT_PUBLIC_API_BASE_URL`, `NEXT_PUBLIC_NETMETRIC_COOKIE_DOMAIN` | `NEXT_PUBLIC_SUPPORT_URL`, monitoring client DSN | Auth/session cookie and return-url policy must match domain strategy. |
| account-web | `NEXT_PUBLIC_APP_NAME`, `NEXT_PUBLIC_API_BASE_URL`, `NEXT_PUBLIC_NETMETRIC_COOKIE_DOMAIN` | `NEXT_PUBLIC_ACCOUNT_DOCS_URL`                   | Preference sync (theme/locale) depends on parent-domain cookie.       |
| crm-web     | `NEXT_PUBLIC_APP_NAME`, `NEXT_PUBLIC_API_BASE_URL`, `NEXT_PUBLIC_NETMETRIC_COOKIE_DOMAIN` | `NEXT_PUBLIC_CRM_DOCS_URL`                       | Tenant context and membership guard required.                         |
| tools-web   | `NEXT_PUBLIC_APP_NAME`, `NEXT_PUBLIC_API_BASE_URL`, `NEXT_PUBLIC_NETMETRIC_COOKIE_DOMAIN` | `NEXT_PUBLIC_TOOLS_DOCS_URL`                     | Tools history/auth routes require correlation headers.                |
| public-web  | `NEXT_PUBLIC_APP_NAME`, `NEXT_PUBLIC_PUBLIC_BASE_URL`                                     | `NEXT_PUBLIC_STATUS_URL`                         | Public forms must keep spam controls enabled.                         |

## Backend Services

| Service             | Required                                                          | Optional                | Notes                                                 |
| ------------------- | ----------------------------------------------------------------- | ----------------------- | ----------------------------------------------------- |
| auth-api            | DB connection, JWT signing keys, cookie policy, CORS origins      | OTEL endpoint           | Register/login/reset endpoints rate-limited.          |
| account-api         | DB connection, auth issuer/audience, storage settings             | OTEL endpoint           | Profile/media/session endpoints audited.              |
| crm-api             | DB connection, auth policy, tenant resolver, RabbitMQ             | OTEL endpoint, SMTP/SES | Permission and tenant scope controls mandatory.       |
| tools-api           | DB connection, artifact storage path/bucket, auth validation      | OTEL endpoint           | Artifact retention and max upload limits required.    |
| gateway             | downstream route targets, CORS origins, correlation header policy | OTEL endpoint           | Forwards and generates correlation id.                |
| notification-worker | RabbitMQ, SMTP/SES provider, DB connection                        | OTEL endpoint           | `/health/live` and `/health/ready` split is required. |

## Infrastructure Variables

- Cookie domain and security flags: parent domain strategy in cookie doc.
- CORS: explicit allowlists for `netmetric.net` and product subdomains.
- Data: PostgreSQL per bounded context.
- Messaging: RabbitMQ connection and queue policy.
- Monitoring: OTEL endpoint, service name, environment tags.
- Storage: object bucket/container and retention policy.

## Local / Staging / Production Strategy

- Local: permissive developer defaults, no production secrets.
- Staging: production-like topology with synthetic data and strict CORS/cookie config.
- Production: secret manager only, immutable infra manifests, monitored release gate.
