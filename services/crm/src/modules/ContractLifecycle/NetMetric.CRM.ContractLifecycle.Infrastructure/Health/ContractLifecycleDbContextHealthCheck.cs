// <copyright file="ContractLifecycleDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.ContractLifecycle.Infrastructure.Persistence;

namespace NetMetric.CRM.ContractLifecycle.Infrastructure.Health;

public sealed class ContractLifecycleDbContextHealthCheck(ContractLifecycleDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("ContractLifecycle database is reachable.")
            : HealthCheckResult.Unhealthy("ContractLifecycle database is not reachable.");
    }
}
