// <copyright file="IUserTokenStateValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Abstractions;

public interface IUserTokenStateValidator
{
    Task<bool> IsValidAsync(Guid tenantId, Guid userId, int tokenVersion, CancellationToken cancellationToken);

    void Evict(Guid tenantId, Guid userId);
}
