// <copyright file="ToolsDbHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.Tools.Persistence;

namespace NetMetric.Tools.API.Health;

public sealed class ToolsDbHealthCheck(ToolsDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("Tools database is reachable.")
            : HealthCheckResult.Unhealthy("Tools database is not reachable.");
    }
}
