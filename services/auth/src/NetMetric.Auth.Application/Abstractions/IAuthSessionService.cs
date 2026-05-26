// <copyright file="IAuthSessionService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.Domain.Entities;

namespace NetMetric.Auth.Application.Abstractions;

public interface IAuthSessionService
{
    Task<IReadOnlyCollection<Guid>> EnforceSessionLimitsAsync(Guid tenantId, Guid userId, Guid? currentSessionId, CancellationToken cancellationToken);

    bool IsExpired(UserSession session, DateTime utcNow);
}
