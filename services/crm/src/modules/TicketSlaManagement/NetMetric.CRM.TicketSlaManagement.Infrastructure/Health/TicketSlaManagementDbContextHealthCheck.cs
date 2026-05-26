// <copyright file="TicketSlaManagementDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.TicketSlaManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.TicketManagement.Infrastructure.Health;

public sealed class TicketSlaManagementDbContextHealthCheck(TicketSlaManagementDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("TicketSlaManagement database is reachable.")
            : HealthCheckResult.Unhealthy("TicketSlaManagement database is not reachable.");
    }
}
