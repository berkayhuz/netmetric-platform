// <copyright file="TenantContextExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tenancy;

public static class TenantContextExtensions
{
    public static Guid GetRequiredTenantId(this ITenantContext tenantContext)
        => tenantContext.TenantId
            ?? throw new InvalidOperationException("A tenant context is required.");
}
