// <copyright file="IntegrationLogEntry.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.IntegrationHub.Domain.Entities;

public sealed class IntegrationLogEntry : EntityBase
{
    public string ProviderKey { get; private set; } = null!;
    public string Direction { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public int RetryCount { get; private set; }

    private IntegrationLogEntry() { }

    public IntegrationLogEntry(Guid tenantId, string providerKey, string direction, string status, string message, int retryCount)
    {
        TenantId = tenantId;
        ProviderKey = providerKey.Trim();
        Direction = direction.Trim();
        Status = status.Trim();
        Message = message.Trim();
        RetryCount = retryCount;
    }
}
