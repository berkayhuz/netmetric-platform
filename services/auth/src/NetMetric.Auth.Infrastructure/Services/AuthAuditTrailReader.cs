// <copyright file="AuthAuditTrailReader.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.Auth.Application.Abstractions;
using NetMetric.Auth.Infrastructure.Persistence;

namespace NetMetric.Auth.Infrastructure.Services;

public sealed class AuthAuditTrailReader(AuthDbContext dbContext) : IAuthAuditTrailReader
{
    public async Task<DateTimeOffset?> GetLastSecurityEventAtAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        var createdAt = await dbContext.AuthAuditEvents
            .Where(x => x.TenantId == tenantId && x.UserId == userId && !x.IsDeleted && x.EventType.StartsWith("auth."))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => (DateTime?)x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return createdAt.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(createdAt.Value, DateTimeKind.Utc)) : null;
    }
}
