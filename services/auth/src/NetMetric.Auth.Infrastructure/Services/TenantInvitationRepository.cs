// <copyright file="TenantInvitationRepository.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.Auth.Application.Abstractions;
using NetMetric.Auth.Domain.Entities;
using NetMetric.Auth.Infrastructure.Persistence;

namespace NetMetric.Auth.Infrastructure.Services;

public sealed class TenantInvitationRepository(AuthDbContext dbContext) : ITenantInvitationRepository
{
    public Task<TenantInvitation?> GetPendingByTokenHashAsync(
        Guid tenantId,
        string tokenHash,
        DateTime utcNow,
        CancellationToken cancellationToken) =>
        dbContext.TenantInvitations.SingleOrDefaultAsync(
            x => x.TenantId == tenantId &&
                 x.TokenHash == tokenHash &&
                 x.AcceptedAtUtc == null &&
                 x.ExpiresAtUtc > utcNow &&
                 !x.IsDeleted &&
                 x.IsActive,
            cancellationToken);

    public Task<bool> HasPendingInviteForEmailAsync(
        Guid tenantId,
        string normalizedEmail,
        DateTime utcNow,
        CancellationToken cancellationToken) =>
        dbContext.TenantInvitations.AnyAsync(
            x => x.TenantId == tenantId &&
                 x.NormalizedEmail == normalizedEmail &&
                 x.AcceptedAtUtc == null &&
                 x.ExpiresAtUtc > utcNow &&
                 !x.IsDeleted &&
                 x.IsActive,
            cancellationToken);

    public async Task<TenantInvitation?> GetByIdAsync(Guid tenantId, Guid invitationId, CancellationToken cancellationToken) =>
        await dbContext.TenantInvitations.SingleOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == invitationId,
            cancellationToken);

    public async Task<IReadOnlyCollection<TenantInvitation>> ListForTenantAsync(Guid tenantId, CancellationToken cancellationToken) =>
        await dbContext.TenantInvitations
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TenantInvitation invitation, CancellationToken cancellationToken) =>
        await dbContext.TenantInvitations.AddAsync(invitation, cancellationToken);
}
