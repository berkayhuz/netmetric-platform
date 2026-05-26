// <copyright file="SalesForecastingDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.SalesForecasting.Infrastructure.Persistence;

namespace NetMetric.CRM.SalesForecasting.Infrastructure.Health;

public sealed class SalesForecastingDbContextHealthCheck(SalesForecastingDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("SalesForecasting database is reachable.")
            : HealthCheckResult.Unhealthy("SalesForecasting database is not reachable.");
    }
}
