// <copyright file="AccountPendingMigrationsHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.Account.Persistence;

namespace NetMetric.Account.Api.Health;

public sealed class AccountPendingMigrationsHealthCheck(AccountDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var pendingMigrations = await dbContext.Database
            .GetPendingMigrationsAsync(cancellationToken);

        var pending = pendingMigrations.ToArray();
        return pending.Length == 0
            ? HealthCheckResult.Healthy("No pending Account database migrations.")
            : HealthCheckResult.Unhealthy($"Pending Account database migrations detected: {string.Join(',', pending)}");
    }
}
