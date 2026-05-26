// <copyright file="TrustedDeviceRepository.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.Auth.Application.Abstractions;
using NetMetric.Auth.Domain.Entities;
using NetMetric.Auth.Infrastructure.Persistence;

namespace NetMetric.Auth.Infrastructure.Services;

public sealed class TrustedDeviceRepository(AuthDbContext dbContext) : ITrustedDeviceRepository
{
    public async Task<IReadOnlyCollection<TrustedDevice>> ListForUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken) =>
        await dbContext.TrustedDevices
            .Where(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted)
            .OrderByDescending(x => x.LastSeenAtUtc ?? x.TrustedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<bool> RevokeAsync(Guid tenantId, Guid userId, Guid deviceId, DateTime utcNow, string reason, CancellationToken cancellationToken)
    {
        var device = await dbContext.TrustedDevices.SingleOrDefaultAsync(
            x => x.TenantId == tenantId && x.UserId == userId && x.Id == deviceId && !x.IsDeleted,
            cancellationToken);

        if (device is null)
        {
            return false;
        }

        if (!device.IsRevoked)
        {
            device.Revoke(utcNow, reason);
        }

        return true;
    }
}
