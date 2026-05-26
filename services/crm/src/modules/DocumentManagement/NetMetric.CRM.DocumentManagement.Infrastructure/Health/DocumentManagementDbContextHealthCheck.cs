// <copyright file="DocumentManagementDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.DocumentManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.DocumentManagement.Infrastructure.Health;

public sealed class DocumentManagementDbContextHealthCheck : IHealthCheck
{
    private readonly DocumentManagementDbContext _dbContext;

    public DocumentManagementDbContextHealthCheck(DocumentManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("DocumentManagement database is reachable.")
            : HealthCheckResult.Unhealthy("DocumentManagement database is not reachable.");
    }
}
