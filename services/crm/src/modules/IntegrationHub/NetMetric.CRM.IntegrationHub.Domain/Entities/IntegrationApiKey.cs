// <copyright file="IntegrationApiKey.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.IntegrationHub.Domain.Entities;

public sealed class IntegrationApiKey : EntityBase
{
    private IntegrationApiKey() { }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string KeyPrefix { get; private set; } = null!;
    public string KeySalt { get; private set; } = null!;
    public string KeyHash { get; private set; } = null!;
    public DateTime? ExpiresAtUtc { get; private set; }
    public DateTime? LastUsedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public List<IntegrationApiKeyScope> Scopes { get; private set; } = [];

    public IntegrationApiKey(
        Guid tenantId,
        string name,
        string? description,
        string keyPrefix,
        string keySalt,
        string keyHash,
        IEnumerable<string> scopes,
        DateTime? expiresAtUtc)
    {
        TenantId = tenantId;
        Name = Guard.AgainstNullOrWhiteSpace(name).Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        KeyPrefix = Guard.AgainstNullOrWhiteSpace(keyPrefix).Trim();
        KeySalt = Guard.AgainstNullOrWhiteSpace(keySalt).Trim();
        KeyHash = Guard.AgainstNullOrWhiteSpace(keyHash).Trim();
        ExpiresAtUtc = expiresAtUtc;

        foreach (var scope in scopes.Select(x => x.Trim().ToLowerInvariant()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            Scopes.Add(new IntegrationApiKeyScope(scope));
        }
    }

    public bool IsExpired(DateTime nowUtc) => ExpiresAtUtc.HasValue && ExpiresAtUtc.Value <= nowUtc;

    public new bool IsActive(DateTime nowUtc) => !IsRevoked && !IsExpired(nowUtc);

    public void Revoke(DateTime revokedAtUtc)
    {
        RevokedAtUtc = revokedAtUtc;
    }

    public void MarkUsed(DateTime usedAtUtc)
    {
        LastUsedAtUtc = usedAtUtc;
    }
}

public sealed class IntegrationApiKeyScope
{
    private IntegrationApiKeyScope() { }

    public IntegrationApiKeyScope(string scope)
    {
        Id = Guid.NewGuid();
        Scope = Guard.AgainstNullOrWhiteSpace(scope).Trim().ToLowerInvariant();
    }

    public Guid Id { get; private set; }
    public string Scope { get; private set; } = null!;
}
