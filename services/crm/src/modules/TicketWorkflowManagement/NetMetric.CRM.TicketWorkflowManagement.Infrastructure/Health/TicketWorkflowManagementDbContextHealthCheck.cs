// <copyright file="TicketWorkflowManagementDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.TicketWorkflowManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.TicketWorkflowManagement.Infrastructure.Health;

public sealed class TicketWorkflowManagementDbContextHealthCheck(TicketWorkflowManagementDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("Ticket workflow management database is reachable.")
            : HealthCheckResult.Unhealthy("Ticket workflow management database is not reachable.");
    }
}
