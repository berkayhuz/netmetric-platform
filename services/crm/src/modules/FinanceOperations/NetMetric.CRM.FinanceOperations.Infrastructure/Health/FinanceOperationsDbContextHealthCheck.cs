// <copyright file="FinanceOperationsDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.FinanceOperations.Infrastructure.Persistence;

namespace NetMetric.CRM.FinanceOperations.Infrastructure.Health;

public sealed class FinanceOperationsDbContextHealthCheck(FinanceOperationsDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("FinanceOperations database is reachable.")
            : HealthCheckResult.Unhealthy("FinanceOperations database is not reachable.");
    }
}
