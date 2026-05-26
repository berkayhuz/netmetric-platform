// <copyright file="ProductCatalogDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.ProductCatalog.Infrastructure.Persistence;

namespace NetMetric.CRM.ProductCatalog.Infrastructure.Health;

public sealed class ProductCatalogDbContextHealthCheck(ProductCatalogDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("ProductCatalog database is reachable.")
            : HealthCheckResult.Unhealthy("ProductCatalog database is not reachable.");
    }
}
