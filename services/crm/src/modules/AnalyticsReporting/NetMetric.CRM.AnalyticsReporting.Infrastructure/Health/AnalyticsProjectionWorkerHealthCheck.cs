// <copyright file="AnalyticsProjectionWorkerHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NetMetric.CRM.AnalyticsReporting.Domain.Entities;
using NetMetric.CRM.AnalyticsReporting.Infrastructure.Persistence;
using NetMetric.CRM.AnalyticsReporting.Infrastructure.Projection;

namespace NetMetric.CRM.AnalyticsReporting.Infrastructure.Health;

public sealed class AnalyticsProjectionWorkerHealthCheck(
    AnalyticsReportingDbContext dbContext,
    IOptions<AnalyticsProjectionOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
        {
            return HealthCheckResult.Healthy("Analytics projection worker is disabled by configuration.");
        }

        var lastRun = await dbContext.ProjectionRuns
            .AsNoTracking()
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastRun is null)
        {
            return HealthCheckResult.Degraded("Analytics projection worker has not completed a run yet.");
        }

        if (lastRun.Status == AnalyticsProjectionRunStatus.Failed)
        {
            return HealthCheckResult.Unhealthy("The last analytics projection run failed.", data: new Dictionary<string, object>
            {
                ["correlationId"] = lastRun.CorrelationId,
                ["startedAtUtc"] = lastRun.StartedAtUtc,
                ["error"] = lastRun.ErrorMessage ?? "Unknown failure"
            });
        }

        var maxAge = TimeSpan.FromSeconds(Math.Max(60, options.Value.IntervalSeconds) * 3);
        if (lastRun.CompletedAtUtc is null || DateTime.UtcNow - lastRun.CompletedAtUtc > maxAge)
        {
            return HealthCheckResult.Degraded("Analytics projection worker is stale.", data: new Dictionary<string, object>
            {
                ["correlationId"] = lastRun.CorrelationId,
                ["completedAtUtc"] = lastRun.CompletedAtUtc ?? lastRun.StartedAtUtc
            });
        }

        return HealthCheckResult.Healthy("Analytics projection worker is current.", new Dictionary<string, object>
        {
            ["correlationId"] = lastRun.CorrelationId,
            ["completedAtUtc"] = lastRun.CompletedAtUtc.Value,
            ["projectedTenantCount"] = lastRun.ProjectedTenantCount
        });
    }
}
