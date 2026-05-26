// <copyright file="GetDealsQueryHandler.cs" company="NetMetric">
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
using NetMetric.Pagination;

namespace NetMetric.CRM.DealManagement.Application.Handlers;

public sealed class GetDealsQueryHandler(
    IDealManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetDealsQuery, PagedResult<DealListItemDto>>
{
    public async Task<PagedResult<DealListItemDto>> Handle(GetDealsQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.DealsResource);
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var page = new DealPageRequest(request.Page, request.PageSize);
        var query = dbContext.Deals
            .AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.DealCode.Contains(search) || x.Name.Contains(search));
        }

        if (request.OwnerUserId.HasValue)
            query = query.Where(x => x.OwnerUserId == request.OwnerUserId);
        if (request.CompanyId.HasValue)
            query = query.Where(x => x.CompanyId == request.CompanyId);
        if (request.OpportunityId.HasValue)
            query = query.Where(x => x.OpportunityId == request.OpportunityId);
        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.ClosedDate).ThenBy(x => x.Name)
            .Skip((page.NormalizedPage - 1) * page.NormalizedPageSize)
            .Take(page.NormalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<DealListItemDto>
        {
            Items = items.Select(x => x.ToListItemDto(canSeeFinancialData)).ToList(),
            TotalCount = totalCount,
            PageNumber = page.NormalizedPage,
            PageSize = page.NormalizedPageSize
        };
    }
}
