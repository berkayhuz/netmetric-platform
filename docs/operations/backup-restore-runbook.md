# Backup and Restore Runbook

## Scope

- PostgreSQL databases (auth/account/crm/tools)
- Object storage (media, tools artifacts)
- Secrets/config (Kubernetes secrets, env sources)
- EF migration rollback strategy
- Disaster recovery (DR) validation

## Environment Strategy

- Local: daily logical dump, 7-day retention, optional encryption.
- Staging: every 6 hours logical dump + object storage versioning, 14-day retention.
- Production: hourly WAL/incremental + daily full backup, 35-day retention, encrypted at rest and in transit.

## PostgreSQL Backup

1. Verify primary DB endpoint and credential source from secret store.
2. Run logical dump (`pg_dump`) per service DB.
3. Run global metadata dump (`pg_dumpall --globals-only`).
4. Store to immutable bucket prefix by date.
5. Record checksum and backup manifest.

## Object Storage Backup

1. Sync media/artifact prefixes to backup bucket.
2. Preserve object metadata and version history.
3. Validate sample restore of at least one artifact per app.

## Secrets and Config Backup

1. Export Kubernetes secret manifests without plaintext values.
2. Back up secret references and key names from secret manager.
3. Back up configmaps and environment manifests.

## Restore Procedure

1. Select recovery point objective (RPO) timestamp.
2. Provision isolated restore environment.
3. Restore PostgreSQL full backup then apply WAL/incrementals.
4. Restore object storage prefixes and verify checksums.
5. Reapply secrets/config references.
6. Run EF migration validation against restored state.

## Migration Rollback

- If deployment migration fails, stop rollout and run last-known-good migration bundle.
- Validate schema compatibility before traffic cutover.
- Capture incident timeline with correlation IDs.

## DR Drill Checklist

- Quarterly restore drill for staging-like environment.
- Verify auth/account/crm/tools smoke endpoints.
- Verify login, tenant selection, CRM list query, tools history read.
- Verify queue consumers and webhook endpoints recover.

## Post-Restore Smoke Commands

- `dotnet test NetMetric.slnx -c Release --no-build -m:1 -v minimal`
- `pnpm lint`
- `pnpm test`
- `pnpm build`
- `pnpm run release:smoke:local`

## Retention and Ownership

- Backup owner: Platform/Operations.
- Restore owner: On-call + service owner.
- Audit trail: every backup/restore action must include operator id, timestamp, and ticket id.
