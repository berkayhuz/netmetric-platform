// <copyright file="GetQuoteTimelineQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Queries.Quotes;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class GetQuoteTimelineQueryHandler(
    IQuoteManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetQuoteTimelineQuery, IReadOnlyList<QuoteTimelineEventDto>>
{
    public async Task<IReadOnlyList<QuoteTimelineEventDto>> Handle(GetQuoteTimelineQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.QuotesResource);
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var allowedQuoteExists = await dbContext.Quotes.AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .AnyAsync(x => x.Id == request.QuoteId, cancellationToken);
        if (!allowedQuoteExists)
        {
            return [];
        }

        var history = await dbContext.QuoteStatusHistories.AsNoTracking().Where(x => x.QuoteId == request.QuoteId).OrderByDescending(x => x.ChangedAt).ToListAsync(cancellationToken);
        return history.Select(x => new QuoteTimelineEventDto(x.ChangedAt, x.NewStatus.ToString(), $"Quote moved to {x.NewStatus}", canSeeInternalNotes ? x.Note : null)).ToList();
    }
}
