# Auth Duplicate Email Remediation

P0 migration `EnforceGlobalUniqueUserEmail` adds a global unique index on `Users.NormalizedEmail`.
The migration intentionally fails fast when duplicate normalized emails already exist. It does not merge,
delete, or reassign identities automatically.

## Precheck

Run before applying the migration in staging or production:

```sql
SELECT NormalizedEmail, COUNT(*) AS DuplicateCount
FROM Users
GROUP BY NormalizedEmail
HAVING COUNT(*) > 1;
```

For investigation:

```sql
SELECT Id, TenantId, UserName, Email, NormalizedEmail, IsActive, IsDeleted, CreatedAt
FROM Users
WHERE NormalizedEmail IN (
    SELECT NormalizedEmail
    FROM Users
    GROUP BY NormalizedEmail
    HAVING COUNT(*) > 1
)
ORDER BY NormalizedEmail, CreatedAt;
```

## Remediation

1. Identify the canonical identity for each duplicate normalized email.
2. Confirm tenant memberships, sessions, audit records, and account profile ownership with the tenant owner or support owner.
3. Manually migrate memberships or dependent records to the canonical user where business-approved.
4. Revoke sessions for duplicate identities that will no longer be usable.
5. Soft-delete or correct non-canonical users only after the manual decision is recorded.
6. Re-run the precheck. Apply the migration only when it returns zero rows.

If duplicates remain, the migration raises a clear SQL error and leaves the existing schema unchanged.
