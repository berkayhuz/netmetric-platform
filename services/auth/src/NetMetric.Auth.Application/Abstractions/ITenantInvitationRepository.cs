// <copyright file="ITenantInvitationRepository.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.Domain.Entities;

namespace NetMetric.Auth.Application.Abstractions;

public interface ITenantInvitationRepository
{
    Task<TenantInvitation?> GetPendingByTokenHashAsync(Guid tenantId, string tokenHash, DateTime utcNow, CancellationToken cancellationToken);
    Task<TenantInvitation?> GetByIdAsync(Guid tenantId, Guid invitationId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TenantInvitation>> ListForTenantAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<bool> HasPendingInviteForEmailAsync(Guid tenantId, string normalizedEmail, DateTime utcNow, CancellationToken cancellationToken);
    Task AddAsync(TenantInvitation invitation, CancellationToken cancellationToken);
}
