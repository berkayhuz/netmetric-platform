// <copyright file="GetGlobalTrashItemsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.CustomerManagement.Application.Queries.Trash;

public sealed class GetGlobalTrashItemsQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope)
    : IRequestHandler<GetGlobalTrashItemsQuery, PagedResult<GlobalTrashItemListItemDto>>
{
    public async Task<PagedResult<GlobalTrashItemListItemDto>> Handle(
        GetGlobalTrashItemsQuery request,
        CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.ContactsResource);
        var pageNumber = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize > 200 ? 200 : request.PageSize;
        var normalizedEntityType = request.EntityType?.Trim().ToLowerInvariant();

        var query = dbContext.Set<GlobalTrashItem>()
            .AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.DeletedByUserId, x => x.DeletedByUserId)
            .Where(x => x.Status == CrmTrashStatuses.Active);

        if (!string.IsNullOrWhiteSpace(normalizedEntityType))
        {
            query = query.Where(x => x.EntityType == normalizedEntityType);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.DisplayName, search) ||
                (x.Summary != null && EF.Functions.Like(x.Summary, search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = await ApplySorting(query, request.SortBy, request.SortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new GlobalTrashItemListItemDto(
                x.Id,
                x.EntityType,
                x.EntityId,
                x.DisplayName,
                x.Summary,
                x.SourceModule,
                x.OriginalRoute,
                x.DeletedAtUtc,
                x.DeletedByUserId,
                x.DeletedByDisplayName,
                x.ExpiresAtUtc,
                x.Status))
            .ToListAsync(cancellationToken);

        return new PagedResult<GlobalTrashItemListItemDto>
        {
            Items = page,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static IOrderedQueryable<GlobalTrashItem> ApplySorting(
        IQueryable<GlobalTrashItem> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

        return normalizedSortBy switch
        {
            "expiresat" => descending
                ? query.OrderByDescending(x => x.ExpiresAtUtc).ThenByDescending(x => x.DeletedAtUtc)
                : query.OrderBy(x => x.ExpiresAtUtc).ThenByDescending(x => x.DeletedAtUtc),
            "entitytype" => descending
                ? query.OrderByDescending(x => x.EntityType).ThenByDescending(x => x.DeletedAtUtc)
                : query.OrderBy(x => x.EntityType).ThenByDescending(x => x.DeletedAtUtc),
            _ => descending
                ? query.OrderByDescending(x => x.DeletedAtUtc).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.DeletedAtUtc).ThenBy(x => x.Id)
        };
    }
}
