// <copyright file="CustomerManagementDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Health;

public sealed class CustomerManagementDbContextHealthCheck(
    CustomerManagementDbContext dbContext) : IHealthCheck
{
    private readonly CustomerManagementDbContext _dbContext = dbContext;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
                return HealthCheckResult.Unhealthy("Customer Management database connection could not be established.");

            return HealthCheckResult.Healthy("Customer Management database is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Customer Management database health check failed.", ex);
        }
    }
}
