// <copyright file="IAuthVerificationTokenRepository.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.Domain.Entities;

namespace NetMetric.Auth.Application.Abstractions;

public interface IAuthVerificationTokenRepository
{
    Task AddAsync(AuthVerificationToken token, CancellationToken cancellationToken);
    Task<AuthVerificationToken?> GetValidAsync(
        Guid tenantId,
        Guid userId,
        string purpose,
        string tokenHash,
        DateTime utcNow,
        CancellationToken cancellationToken);

    Task RevokeOutstandingAsync(
        Guid tenantId,
        Guid userId,
        string purpose,
        DateTime utcNow,
        string? target,
        CancellationToken cancellationToken);
}
