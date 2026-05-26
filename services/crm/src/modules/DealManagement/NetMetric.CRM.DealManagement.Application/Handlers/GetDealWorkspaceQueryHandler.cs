// <copyright file="GetDealWorkspaceQueryHandler.cs" company="NetMetric">
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

public sealed class GetDealWorkspaceQueryHandler(
    IDealManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetDealWorkspaceQuery, DealWorkspaceDto?>
{
    public async Task<DealWorkspaceDto?> Handle(GetDealWorkspaceQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.DealsResource);
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var entity = await dbContext.Deals.AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .FirstOrDefaultAsync(x => x.Id == request.DealId, cancellationToken);
        if (entity is null)
            return null;

        var history = await DealHandlerHelpers.LoadHistoryAsync(dbContext, entity.Id, cancellationToken);
        var review = await DealHandlerHelpers.LoadReviewAsync(dbContext, entity.Id, cancellationToken);
        var lostReasons = await dbContext.LostReasons.AsNoTracking().OrderBy(x => x.Name).Select(x => new LostReasonDto(x.Id, x.Name, x.Description, x.IsDefault)).ToListAsync(cancellationToken);
        return new DealWorkspaceDto(entity.ToDetailDto(review, history, canSeeFinancialData, canSeeInternalNotes), lostReasons, history.Select(x => canSeeInternalNotes ? x.ToDto() : x.ToDto() with { Note = null }).ToList());
    }
}
