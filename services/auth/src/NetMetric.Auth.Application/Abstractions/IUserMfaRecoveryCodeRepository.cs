// <copyright file="IUserMfaRecoveryCodeRepository.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.Domain.Entities;

namespace NetMetric.Auth.Application.Abstractions;

public interface IUserMfaRecoveryCodeRepository
{
    Task<int> CountActiveAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken);
    Task<bool> ConsumeAsync(Guid tenantId, Guid userId, string codeHash, DateTime utcNow, CancellationToken cancellationToken);
    Task ReplaceActiveAsync(Guid tenantId, Guid userId, IReadOnlyCollection<UserMfaRecoveryCode> recoveryCodes, DateTime utcNow, CancellationToken cancellationToken);
    Task RevokeActiveAsync(Guid tenantId, Guid userId, DateTime utcNow, CancellationToken cancellationToken);
}
