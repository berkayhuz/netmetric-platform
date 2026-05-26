// <copyright file="IntegrationHubDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.IntegrationHub.Infrastructure.Persistence;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Health;

public sealed class IntegrationHubDbContextHealthCheck(IntegrationHubDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("IntegrationHub database is reachable.")
            : HealthCheckResult.Unhealthy("IntegrationHub database is not reachable.");
    }
}
