# Tools P1-46..P1-51 Hardening Notes

## Upload Security Policy

- Upload path now validates content by magic bytes (`PNG/JPEG/WebP/PDF`) and rejects MIME mismatch between declared and detected content.
- Size and format processing limits are configuration-driven under `Tools:UploadSecurity`.
- Safe file name normalization remains in place.
- Security scanner extension point is available via `IToolsFileSecurityScanner`. Default implementation is `NoOpToolsFileSecurityScanner`.

## Validation Standard

- Required checks: magic-byte signature, declared MIME vs detected MIME equality, image structural validation (dimension extraction), PDF page marker count limit.
- `IToolsUploadSecurityValidator` returns detected MIME, safe file name, checksum for downstream storage.

## Artifact Storage Strategy

- Storage provider is now config-controlled (`Tools:ArtifactStorage:Provider`): `Local` or `ObjectStorage`.
- Local remains valid for development.
- Production with local storage fails fast unless `AllowUnsafeLocalInProduction=true`.
- Object storage adapter exists as a production extension point and must be implemented with vendor SDK in deployment environments.

## Artifact Metadata and Download Contract

- Artifact writes now persist MIME + filename + checksum sidecar metadata in local storage.
- Download endpoint returns stored MIME, safe filename content disposition, and security headers:
  - `X-Content-Type-Options: nosniff`
  - `Cache-Control: private, max-age=300`

## Retention / Cleanup Policy

- Config section: `Tools:Retention`
- Background cleanup service (`ToolsArtifactCleanupService`) deletes expired/old artifacts and retries failed storage deletes on next run.
- Soft-deleted entries remain hidden through existing `DeletedAtUtc == null` query filters.

## Catalog Authorization Contract

- Public catalog endpoint: `GET /api/v1/tools/catalog`
  - `[AllowAnonymous]`
  - rate-limited with policy `tools-public-catalog`
  - filtered as public-only (`IsEnabled` only)
- Private catalog endpoint: `GET /api/v1/tools/catalog/private`
  - `[Authorize]` with Tools history read policy

## Async Job Model

- Added in-memory async queue for tool run creation:
  - `POST /api/v1/tools/jobs` -> `queued`
  - `GET /api/v1/tools/jobs/{jobId}` -> `queued/running/succeeded/failed`
- Worker runs in background and creates tool history artifact through existing `CreateMyToolRunCommand`.
- This is development-safe; production should replace queue with durable backend (RabbitMQ/Hangfire/worker).

## Production/Staging and Kubernetes Notes

- Multi-replica reliability requires non-local shared storage (`ObjectStorage` implementation or shared PVC).
- Current production config defaults to `ObjectStorage` provider and blocks unsafe local-only storage unless explicitly opted in.
