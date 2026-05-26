// <copyright file="CustomerIntelligenceDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.CustomerIntelligence.Infrastructure.Persistence;

namespace NetMetric.CRM.CustomerIntelligence.Infrastructure.Health;

public sealed class CustomerIntelligenceDbContextHealthCheck : IHealthCheck
{
    private readonly CustomerIntelligenceDbContext _dbContext;

    public CustomerIntelligenceDbContextHealthCheck(CustomerIntelligenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("CustomerIntelligence database is reachable.")
            : HealthCheckResult.Unhealthy("CustomerIntelligence database is not reachable.");
    }
}
