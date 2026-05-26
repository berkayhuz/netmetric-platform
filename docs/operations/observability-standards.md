# Observability Standards (P2-75 to P2-79)

## Correlation and Trace Propagation

- Standard request correlation header: `X-Correlation-Id`.
- Gateway, Auth API, Account API, and CRM API normalize/generate correlation id when incoming header is missing.
- Correlation id is echoed back on response headers and placed into structured log scope with `CorrelationId` and `TraceId`.
- Gateway forwards `X-Correlation-Id` to downstream services and keeps W3C `traceparent` propagation through OpenTelemetry HTTP instrumentation.
- Outgoing integration/outbox flows store correlation id fields (`CorrelationId`) so worker and downstream consumers preserve request lineage.
- Correlation fields must not include secrets/PII values.

## Frontend Error Monitoring Provider Standard

- Shared package: `@netmetric/observability`.
- All frontend apps (`auth-web`, `account-web`, `crm-web`, `tools-web`, `public-web`) install global monitoring bootstrap.
- Events captured:
  - `window.error`
  - `window.unhandledrejection`
- Provider selection (env-driven):
  - `NEXT_PUBLIC_ERROR_MONITORING_PROVIDER=console|noop`
  - `NEXT_PUBLIC_APP_ENV=development|test|staging|production`
- `development` and `test` always force `noop` provider.
- Staging/production allows configured provider; missing/`noop` provider emits explicit warning.
- Sensitive fields are redacted before emission (email, token, secret, password, cookie, ip-like keys).

## Audit Log vs Application Log Separation

- Audit events are written through dedicated audit abstractions (`IAuthAuditTrail`, `IAccountAuditWriter`) and persisted to audit stores.
- Audit pipeline logs use `AUDIT_EVENT` marker and do not reuse generic application event names.
- Audit payloads mask sensitive identity/metadata fields before persistence/logging.
- Retention/access:
  - Audit storage is treated as security data with restricted access and longer retention than application logs.
  - Application logs remain operational diagnostics with standard retention.
- Failure behavior:
  - Audit persistence failures are treated as high-severity operational incidents and must trigger alerts.
  - Business-request behavior follows service-level policies; document per service runbook if fail-open/fail-closed is configured.

## Notification Worker Health and Queue Metrics

- Health split:
  - `live`: worker process health.
  - `ready`: RabbitMQ reachability.
- Queue metrics exported via OpenTelemetry meter `NetMetric.Notification.Worker`:
  - `notification.worker.queue.depth`
  - `notification.worker.queue.dead_letter.depth`
  - `notification.worker.queue.consumer_lag`
  - `notification.worker.retry.total`
  - `notification.worker.dead_letter.total`
  - `notification.worker.delivery.latency.ms`
- RabbitMQ unavailable state returns unhealthy readiness.

## Rate-Limit Metrics and Dashboard/Runbook

- Standard tags for rate-limit rejection counters:
  - `endpoint`
  - `policy`
  - `reason`
  - `tenant_hash`
  - `environment`
- Services covered:
  - Gateway (`gateway.rate_limit.rejected`)
  - Auth (`auth.rate_limit.rejected`)
  - Account (`account.rate_limit.rejected`)
  - CRM (`crm.rate_limit.rejected`)
- PII protection:
  - No raw email/token/IP in metric tags.
  - Tenant dimension is hash-based (`tenant_hash`).
- Dashboard guidance:
  - Graph per-service rejection rate and stacked by `policy`.
  - Add alert for sudden rejection spikes in `auth`, `crm-public-capture`, and `crm-webhook` paths.
  - Correlate spikes with `CorrelationId` in service logs for incident triage.

## Production/Staging Impact

- New metrics increase cardinality slightly due to endpoint/policy tags; keep endpoint set controlled.
- Monitoring provider in staging/production requires explicit environment configuration to avoid no-op behavior.
- Queue snapshot reads add lightweight passive queue checks; verify RabbitMQ permissions allow passive declare.
- Audit logs now include explicit audit marker for downstream SIEM routing/filtering.
