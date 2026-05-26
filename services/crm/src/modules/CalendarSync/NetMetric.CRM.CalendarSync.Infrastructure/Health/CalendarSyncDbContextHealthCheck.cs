// <copyright file="CalendarSyncDbContextHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.CalendarSync.Infrastructure.Persistence;

namespace NetMetric.CRM.CalendarSync.Infrastructure.Health;

public sealed class CalendarSyncDbContextHealthCheck(CalendarSyncDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        => await dbContext.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Calendar sync database is unavailable.");
}
