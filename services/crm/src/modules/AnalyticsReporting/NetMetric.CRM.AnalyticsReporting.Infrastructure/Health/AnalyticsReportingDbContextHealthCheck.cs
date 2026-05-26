// <copyright file="AnalyticsReportingDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.AnalyticsReporting.Infrastructure.Persistence;

namespace NetMetric.CRM.AnalyticsReporting.Infrastructure.Health;

public sealed class AnalyticsReportingDbContextHealthCheck(AnalyticsReportingDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("AnalyticsReporting database is reachable.")
            : HealthCheckResult.Unhealthy("AnalyticsReporting database is not reachable.");
    }
}
