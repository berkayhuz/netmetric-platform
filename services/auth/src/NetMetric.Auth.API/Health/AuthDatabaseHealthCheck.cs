// <copyright file="AuthDatabaseHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.Auth.Infrastructure.Persistence;

namespace NetMetric.Auth.API.Health;

public sealed class AuthDatabaseHealthCheck(AuthDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await dbContext.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy("Identity database is reachable.")
            : HealthCheckResult.Unhealthy("Identity database is unreachable.");
    }
}
