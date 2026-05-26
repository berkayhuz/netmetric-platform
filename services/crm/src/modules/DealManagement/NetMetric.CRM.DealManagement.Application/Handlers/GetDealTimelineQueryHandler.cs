// <copyright file="GetDealTimelineQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Application.Common;
using NetMetric.CRM.DealManagement.Application.Queries.Deals;
using NetMetric.CRM.DealManagement.Contracts.DTOs;

namespace NetMetric.CRM.DealManagement.Application.Handlers;

public sealed class GetDealTimelineQueryHandler(
    IDealManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetDealTimelineQuery, IReadOnlyList<DealOutcomeHistoryDto>>
{
    public async Task<IReadOnlyList<DealOutcomeHistoryDto>> Handle(GetDealTimelineQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.DealsResource);
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var allowedDealExists = await dbContext.Deals.AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .AnyAsync(x => x.Id == request.DealId, cancellationToken);
        if (!allowedDealExists)
        {
            return [];
        }

        return (await DealHandlerHelpers.LoadHistoryAsync(dbContext, request.DealId, cancellationToken))
            .Select(x => canSeeInternalNotes ? x.ToDto() : x.ToDto() with { Note = null })
            .ToList();
    }
}
