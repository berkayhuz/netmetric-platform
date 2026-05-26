// <copyright file="GetAccountAuditEntriesQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Domain.Audit;
using NetMetric.Account.Domain.Common;

namespace NetMetric.Account.Application.Audit.Queries;

public sealed record GetAccountAuditEntriesQuery(int Limit = 50, string? EventType = null)
    : IRequest<IReadOnlyCollection<AccountAuditReadModel>>;

public sealed class GetAccountAuditEntriesQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IRepository<IAccountDbContext, AccountAuditEntry> auditEntries)
    : IRequestHandler<GetAccountAuditEntriesQuery, IReadOnlyCollection<AccountAuditReadModel>>
{
    public async Task<IReadOnlyCollection<AccountAuditReadModel>> Handle(
        GetAccountAuditEntriesQuery request,
        CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var limit = request.Limit < 1 ? 50 : Math.Min(request.Limit, 200);
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);

        var query = auditEntries.Query
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(request.EventType))
        {
            var eventType = request.EventType.Trim();
            query = query.Where(x => x.EventType == eventType);
        }

        return await query
            .OrderByDescending(x => x.OccurredAt)
            .Take(limit)
            .Select(x => new AccountAuditReadModel(
                x.Id,
                x.TenantId.Value,
                x.UserId.Value,
                x.EventType,
                x.Severity.ToString(),
                x.OccurredAt,
                x.CorrelationId,
                x.MetadataJson))
            .ToListAsync(cancellationToken);
    }
}
