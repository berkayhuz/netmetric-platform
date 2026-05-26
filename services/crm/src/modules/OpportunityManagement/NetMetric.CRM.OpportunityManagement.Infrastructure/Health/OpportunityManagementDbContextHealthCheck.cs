// <copyright file="OpportunityManagementDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Health;

public sealed class OpportunityManagementDbContextHealthCheck(OpportunityManagementDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("OpportunityManagement database is reachable.")
            : HealthCheckResult.Unhealthy("OpportunityManagement database is not reachable.");
    }
}
