// <copyright file="IntegrationJobWorkerHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.CRM.IntegrationHub.Infrastructure.Persistence;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Health;

public sealed class IntegrationJobWorkerHealthCheck(
    IntegrationHubDbContext dbContext,
    IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue<bool>("Crm:Features:IntegrationJobProcessingEnabled"))
        {
            return HealthCheckResult.Healthy("Integration job processing is disabled by feature flag.");
        }

        var now = DateTime.UtcNow;
        var staleProcessingJobs = await dbContext.IntegrationJobs
            .IgnoreQueryFilters()
            .CountAsync(
                x => x.Status == IntegrationJobStatuses.Processing &&
                     x.LeaseExpiresAtUtc != null &&
                     x.LeaseExpiresAtUtc < now,
                cancellationToken);

        return staleProcessingJobs == 0
            ? HealthCheckResult.Healthy("Integration job worker has no stale leased jobs.")
            : HealthCheckResult.Degraded($"Integration job worker has {staleProcessingJobs} stale leased job(s).");
    }
}
