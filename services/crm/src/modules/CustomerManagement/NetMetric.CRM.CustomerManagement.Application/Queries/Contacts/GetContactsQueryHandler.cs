// <copyright file="GetContactsQueryHandler.cs" company="NetMetric">
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

namespace NetMetric.CRM.CustomerManagement.Application.Queries.Contacts;

public sealed class GetContactsQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService)
    : IRequestHandler<GetContactsQuery, PagedResult<ContactListItemDto>>
{
    public async Task<PagedResult<ContactListItemDto>> Handle(
        GetContactsQuery request,
        CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.ContactsResource);
        var canSeeContactData = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.ContactsResource, "contactData", scope.Permissions)
            .Visibility == FieldVisibility.Visible;
        var pageNumber = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize > 200 ? 200 : request.PageSize;

        var query = dbContext.Contacts
            .AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.FirstName, search) ||
                EF.Functions.Like(x.LastName, search) ||
                (canSeeContactData && x.Email != null && EF.Functions.Like(x.Email, search)) ||
                (canSeeContactData && x.MobilePhone != null && EF.Functions.Like(x.MobilePhone, search)));
        }

        if (request.CompanyId.HasValue)
            query = query.Where(x => x.CompanyId == request.CompanyId.Value);

        if (request.CustomerId.HasValue)
            query = query.Where(x => x.CustomerId == request.CustomerId.Value);

        if (request.IsPrimary.HasValue)
            query = query.Where(x => x.IsPrimaryContact == request.IsPrimary.Value);

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var page = await ApplySorting(query, request.SortBy, request.SortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.Email,
                x.MobilePhone,
                CompanyName = x.Company == null ? null : x.Company.Name,
                CustomerFullName = x.Customer == null ? null : x.Customer.FullName,
                x.IsPrimaryContact,
                x.IsActive,
                x.RowVersion
            })
            .ToListAsync(cancellationToken);

        var items = page
            .Select(x => new ContactListItemDto(
                x.Id,
                x.FullName,
                canSeeContactData ? x.Email : null,
                canSeeContactData ? x.MobilePhone : null,
                x.CompanyName,
                x.CustomerFullName,
                x.IsPrimaryContact,
                x.IsActive,
                Convert.ToBase64String(x.RowVersion)))
            .ToList();

        return new PagedResult<ContactListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static IOrderedQueryable<Contact> ApplySorting(
        IQueryable<Contact> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

        return normalizedSortBy switch
        {
            "email" => descending
                ? query.OrderByDescending(x => x.Email).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)
                : query.OrderBy(x => x.Email).ThenBy(x => x.FirstName).ThenBy(x => x.LastName),
            "createdat" => descending
                ? query.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)
                : query.OrderBy(x => x.CreatedAt).ThenBy(x => x.FirstName).ThenBy(x => x.LastName),
            "company" => descending
                ? query.OrderByDescending(x => x.Company == null ? null : x.Company.Name).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)
                : query.OrderBy(x => x.Company == null ? null : x.Company.Name).ThenBy(x => x.FirstName).ThenBy(x => x.LastName),
            "customer" => descending
                ? query.OrderByDescending(x => x.Customer == null ? null : x.Customer.FirstName).ThenByDescending(x => x.Customer == null ? null : x.Customer.LastName)
                : query.OrderBy(x => x.Customer == null ? null : x.Customer.FirstName).ThenBy(x => x.Customer == null ? null : x.Customer.LastName),
            _ => descending
                ? query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName)
                : query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
        };
    }
}
