// <copyright file="TagManagementDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.TagManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.TagManagement.Infrastructure.Health;

public sealed class TagManagementDbContextHealthCheck : IHealthCheck
{
    private readonly TagManagementDbContext _dbContext;

    public TagManagementDbContextHealthCheck(TagManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("TagManagement database is reachable.")
            : HealthCheckResult.Unhealthy("TagManagement database is not reachable.");
    }
}
