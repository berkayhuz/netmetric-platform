// <copyright file="GetQuotesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Application.Queries.Quotes;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class GetQuotesQueryHandler(
    IQuoteManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetQuotesQuery, PagedResult<QuoteListItemDto>>
{
    public async Task<PagedResult<QuoteListItemDto>> Handle(GetQuotesQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.QuotesResource);
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var query = dbContext.Quotes
            .AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.QuoteNumber.Contains(search) || (x.ProposalTitle != null && x.ProposalTitle.Contains(search)));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);
        if (request.OpportunityId.HasValue)
            query = query.Where(x => x.OpportunityId == request.OpportunityId.Value);
        if (request.CustomerId.HasValue)
            query = query.Where(x => x.CustomerId == request.CustomerId.Value);
        if (request.OwnerUserId.HasValue)
            query = query.Where(x => x.OwnerUserId == request.OwnerUserId.Value);
        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 200);

        var items = await query.OrderByDescending(x => x.QuoteDate).ThenBy(x => x.QuoteNumber).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<QuoteListItemDto>
        {
            Items = items.Select(x => x.ToListItemDto(canSeeFinancialData)).ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }
}
