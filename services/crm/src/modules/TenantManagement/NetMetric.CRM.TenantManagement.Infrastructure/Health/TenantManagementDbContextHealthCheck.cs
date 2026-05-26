// <copyright file="TenantManagementDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.TenantManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.TenantManagement.Infrastructure.Health;

public sealed class TenantManagementDbContextHealthCheck(TenantManagementDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("TenantManagement database is reachable.")
            : HealthCheckResult.Unhealthy("TenantManagement database is not reachable.");
    }
}
