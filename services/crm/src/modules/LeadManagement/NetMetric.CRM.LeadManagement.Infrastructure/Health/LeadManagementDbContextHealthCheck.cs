// <copyright file="LeadManagementDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.LeadManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Health;

public sealed class LeadManagementDbContextHealthCheck(LeadManagementDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("LeadManagement database is unreachable.");
    }
}
