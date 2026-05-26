// <copyright file="GetTicketsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketManagement.Application.Common;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.TicketManagement.Application.Queries.Tickets;

public sealed class GetTicketsQueryHandler(
    ITicketManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope) : IRequestHandler<GetTicketsQuery, PagedResult<TicketListItemDto>>
{
    public async Task<PagedResult<TicketListItemDto>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.TicketsResource);

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 200);

        var query = dbContext.Tickets
            .AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.AssignedUserId, x => x.AssignedUserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.TicketNumber.Contains(search) || x.Subject.Contains(search));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.Priority.HasValue)
            query = query.Where(x => x.Priority == request.Priority.Value);

        if (request.AssignedUserId.HasValue)
            query = query.Where(x => x.AssignedUserId == request.AssignedUserId.Value);

        if (request.CustomerId.HasValue)
            query = query.Where(x => x.CustomerId == request.CustomerId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.OpenedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.ToListItemDto())
            .ToListAsync(cancellationToken);

        return new PagedResult<TicketListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }
}
