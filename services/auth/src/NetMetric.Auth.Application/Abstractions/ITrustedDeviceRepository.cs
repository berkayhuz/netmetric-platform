// <copyright file="ITrustedDeviceRepository.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.Domain.Entities;

namespace NetMetric.Auth.Application.Abstractions;

public interface ITrustedDeviceRepository
{
    Task<IReadOnlyCollection<TrustedDevice>> ListForUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken);
    Task<bool> RevokeAsync(Guid tenantId, Guid userId, Guid deviceId, DateTime utcNow, string reason, CancellationToken cancellationToken);
}
