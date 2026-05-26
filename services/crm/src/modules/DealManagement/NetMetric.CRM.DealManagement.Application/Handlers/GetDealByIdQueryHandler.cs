// <copyright file="GetDealByIdQueryHandler.cs" company="NetMetric">
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

public sealed class GetDealByIdQueryHandler(
    IDealManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetDealByIdQuery, DealDetailDto?>
{
    public async Task<DealDetailDto?> Handle(GetDealByIdQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.DealsResource);
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var entity = await dbContext.Deals.AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .FirstOrDefaultAsync(x => x.Id == request.DealId, cancellationToken);
        if (entity is null)
            return null;
        return entity.ToDetailDto(await DealHandlerHelpers.LoadReviewAsync(dbContext, entity.Id, cancellationToken), await DealHandlerHelpers.LoadHistoryAsync(dbContext, entity.Id, cancellationToken), canSeeFinancialData, canSeeInternalNotes);
    }
}
