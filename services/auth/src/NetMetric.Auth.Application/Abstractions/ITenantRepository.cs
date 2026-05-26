// <copyright file="ITenantRepository.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.Domain.Entities;

namespace NetMetric.Auth.Application.Abstractions;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken);
    Task DeactivateAsync(Tenant tenant, DateTime utcNow, Guid actorUserId, CancellationToken cancellationToken);
}
