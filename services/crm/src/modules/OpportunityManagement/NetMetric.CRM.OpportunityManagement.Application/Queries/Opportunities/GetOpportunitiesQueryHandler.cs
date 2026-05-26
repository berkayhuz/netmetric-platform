// <copyright file="GetOpportunitiesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Application.Common;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.OpportunityManagement.Application.Queries.Opportunities;

public sealed class GetOpportunitiesQueryHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetOpportunitiesQuery, PagedResult<OpportunityListItemDto>>
{
    public async Task<PagedResult<OpportunityListItemDto>> Handle(GetOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.OpportunitiesResource);
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize > 200 ? 200 : request.PageSize;
        var query = dbContext.Opportunities
            .AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Name.Contains(search) || x.OpportunityCode.Contains(search));
        }

        if (request.Stage.HasValue) query = query.Where(x => x.Stage == request.Stage.Value);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.OwnerUserId.HasValue) query = query.Where(x => x.OwnerUserId == request.OwnerUserId.Value);
        if (request.LeadId.HasValue) query = query.Where(x => x.LeadId == request.LeadId.Value);
        if (request.CustomerId.HasValue) query = query.Where(x => x.CustomerId == request.CustomerId.Value);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<OpportunityListItemDto> { Items = items.Select(x => x.ToListItemDto(canSeeFinancialData)).ToList(), TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
    }
}
