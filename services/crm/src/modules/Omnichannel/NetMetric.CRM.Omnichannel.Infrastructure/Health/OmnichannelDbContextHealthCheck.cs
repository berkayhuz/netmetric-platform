// <copyright file="OmnichannelDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.Omnichannel.Infrastructure.Persistence;

namespace NetMetric.CRM.Omnichannel.Infrastructure.Health;

public sealed class OmnichannelDbContextHealthCheck(OmnichannelDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => await dbContext.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Omnichannel database is unavailable.");
}
