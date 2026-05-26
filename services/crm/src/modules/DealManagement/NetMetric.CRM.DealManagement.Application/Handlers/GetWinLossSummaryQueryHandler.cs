// <copyright file="GetWinLossSummaryQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Application.Queries.Deals;
using NetMetric.CRM.DealManagement.Contracts.DTOs;

namespace NetMetric.CRM.DealManagement.Application.Handlers;

public sealed class GetWinLossSummaryQueryHandler(
    IDealManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope) : IRequestHandler<GetWinLossSummaryQuery, WinLossSummaryDto>
{
    public async Task<WinLossSummaryDto> Handle(GetWinLossSummaryQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.DealsResource);
        var query = dbContext.Deals.AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId);
        if (request.From.HasValue)
            query = query.Where(x => x.ClosedDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(x => x.ClosedDate <= request.To.Value);
        if (request.OwnerUserId.HasValue)
            query = query.Where(x => x.OwnerUserId == request.OwnerUserId.Value);

        var deals = await query.ToListAsync(cancellationToken);
        var histories = await dbContext.DealOutcomeHistories.AsNoTracking().Where(x => deals.Select(d => d.Id).Contains(x.DealId)).ToListAsync(cancellationToken);
        var reasons = await dbContext.LostReasons.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var wonDeals = deals.Count(x => x.TotalAmount > 0m);
        var lostDeals = deals.Count(x => x.TotalAmount <= 0m);
        var wonAmount = deals.Where(x => x.TotalAmount > 0m).Sum(x => x.TotalAmount);
        var lostAmount = deals.Where(x => x.TotalAmount <= 0m).Sum(x => x.TotalAmount);

        var breakdown = histories
            .Where(x => string.Equals(x.Outcome, "Lost", StringComparison.OrdinalIgnoreCase))
            .GroupJoin(deals, history => history.DealId, deal => deal.Id, (history, matchedDeals) => new { history, matchedDeals = matchedDeals.DefaultIfEmpty().FirstOrDefault() })
            .GroupBy(x => x.history.LostReasonId)
            .Select(group => new LostReasonBreakdownDto(group.Key, group.Key.HasValue && reasons.TryGetValue(group.Key.Value, out var label) ? label : "Unknown", group.Count(), group.Where(x => x.matchedDeals is not null).Sum(x => x.matchedDeals!.TotalAmount)))
            .OrderByDescending(x => x.Count)
            .ToList();

        return new WinLossSummaryDto(deals.Count, wonDeals, lostDeals, wonAmount, lostAmount, breakdown);
    }
}
