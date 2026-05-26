// <copyright file="WorkflowAutomationDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Health;

public sealed class WorkflowAutomationDbContextHealthCheck : IHealthCheck
{
    private readonly WorkflowAutomationDbContext _dbContext;

    public WorkflowAutomationDbContextHealthCheck(WorkflowAutomationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("WorkflowAutomation database is reachable.")
            : HealthCheckResult.Unhealthy("WorkflowAutomation database is not reachable.");
    }
}
