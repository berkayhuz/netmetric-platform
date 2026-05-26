// <copyright file="IAuthAuditTrailReader.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Abstractions;

public interface IAuthAuditTrailReader
{
    Task<DateTimeOffset?> GetLastSecurityEventAtAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken);
}
