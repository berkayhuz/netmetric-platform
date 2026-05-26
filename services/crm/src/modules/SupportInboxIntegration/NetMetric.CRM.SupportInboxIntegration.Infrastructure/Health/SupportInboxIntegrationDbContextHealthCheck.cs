// <copyright file="SupportInboxIntegrationDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.SupportInboxIntegration.Infrastructure.Persistence;

namespace NetMetric.CRM.SupportInboxIntegration.Infrastructure.Health;

public sealed class SupportInboxIntegrationDbContextHealthCheck(SupportInboxIntegrationDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("Support inbox integration database is reachable.")
            : HealthCheckResult.Unhealthy("Support inbox integration database is not reachable.");
    }
}
