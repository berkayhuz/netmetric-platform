// <copyright file="TicketManagementDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.TicketManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.TicketManagement.Infrastructure.Health;

public sealed class TicketManagementDbContextHealthCheck(TicketManagementDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("TicketManagement database is reachable.")
            : HealthCheckResult.Unhealthy("TicketManagement database is not reachable.");
    }
}
