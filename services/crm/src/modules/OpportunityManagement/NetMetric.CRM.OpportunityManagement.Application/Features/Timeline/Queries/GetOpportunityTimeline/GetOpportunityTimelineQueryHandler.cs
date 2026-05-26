// <copyright file="GetOpportunityTimelineQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Timeline.Queries.GetOpportunityTimeline;

public sealed class GetOpportunityTimelineQueryHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetOpportunityTimelineQuery, IReadOnlyList<OpportunityTimelineEventDto>>
{
    public async Task<IReadOnlyList<OpportunityTimelineEventDto>> Handle(GetOpportunityTimelineQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.OpportunitiesResource);
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var items = new List<OpportunityTimelineEventDto>();
        var allowedOpportunityExists = await dbContext.Opportunities.AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .AnyAsync(x => x.Id == request.OpportunityId, cancellationToken);
        if (!allowedOpportunityExists)
        {
            return [];
        }

        var stageHistory = await dbContext.OpportunityStageHistories.AsNoTracking().Where(x => x.TenantId == scope.TenantId && x.OpportunityId == request.OpportunityId).OrderByDescending(x => x.ChangedAt).ToListAsync(cancellationToken);
        items.AddRange(stageHistory.Select(x => new OpportunityTimelineEventDto(x.ChangedAt, "stage-change", $"{x.OldStage} → {x.NewStage}", canSeeInternalNotes ? x.Note : null)));

        var quotes = await dbContext.Quotes.AsNoTracking().Where(x => x.TenantId == scope.TenantId && x.OpportunityId == request.OpportunityId).OrderByDescending(x => x.QuoteDate).ToListAsync(cancellationToken);
        items.AddRange(quotes.Select(x => new OpportunityTimelineEventDto(x.QuoteDate, "quote", $"Quote {x.QuoteNumber}", canSeeFinancialData ? $"Grand total: {x.GrandTotal} {x.CurrencyCode}" : null)));

        var activities = await dbContext.Activities.AsNoTracking().Where(x => x.TenantId == scope.TenantId && x.OpportunityId == request.OpportunityId).OrderByDescending(x => x.StartDate).ToListAsync(cancellationToken);
        items.AddRange(activities.Select(x => new OpportunityTimelineEventDto(x.StartDate ?? x.DueAt ?? x.CreatedAt, "activity", x.Subject, canSeeInternalNotes ? x.Description : null)));

        return items.OrderByDescending(x => x.OccurredAt).ToList();
    }
}
