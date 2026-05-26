// <copyright file="GetCustomersQueryHandler.cs" company="NetMetric">
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

namespace NetMetric.CRM.CustomerManagement.Application.Queries.Customers;

public sealed class GetCustomersQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService)
    : IRequestHandler<GetCustomersQuery, PagedResult<CustomerListItemDto>>
{
    public async Task<PagedResult<CustomerListItemDto>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.CustomersResource);
        var canSeeContactData = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.CustomersResource, "contactData", scope.Permissions)
            .Visibility == FieldVisibility.Visible;
        var canSeeIdentityNumber = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.CustomersResource, "identityNumber", scope.Permissions)
            .Visibility == FieldVisibility.Visible;
        var pageNumber = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize > 500 ? 500 : request.PageSize;

        var query = dbContext.Customers
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
                (canSeeContactData && x.MobilePhone != null && EF.Functions.Like(x.MobilePhone, search)) ||
                (canSeeIdentityNumber && x.IdentityNumber != null && EF.Functions.Like(x.IdentityNumber, search)));
        }

        if (request.CustomerType.HasValue)
            query = query.Where(x => x.CustomerType == request.CustomerType.Value);

        if (request.IsVip.HasValue)
            query = query.Where(x => x.IsVip == request.IsVip.Value);

        if (request.CompanyId.HasValue)
            query = query.Where(x => x.CompanyId == request.CompanyId.Value);

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
                x.CustomerType,
                x.IsVip,
                CompanyName = x.Company == null ? null : x.Company.Name,
                x.IsActive,
                x.CreatedAt,
                x.RowVersion,
                x.ProfileImageUrl
            })
            .ToListAsync(cancellationToken);

        var items = page
            .Select(x => new CustomerListItemDto(
                x.Id,
                x.FullName,
                canSeeContactData ? x.Email : null,
                canSeeContactData ? x.MobilePhone : null,
                x.CustomerType,
                x.IsVip,
                x.CompanyName,
                x.IsActive,
                x.CreatedAt,
                Convert.ToBase64String(x.RowVersion),
                x.ProfileImageUrl))
            .ToList();

        return new PagedResult<CustomerListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static IOrderedQueryable<Customer> ApplySorting(
        IQueryable<Customer> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var normalizedSortBy = sortBy?.Trim().ToLowerInvariant();

        return normalizedSortBy switch
        {
            "name" or "fullname" => descending
                ? query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName)
                : query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName),
            "email" => descending
                ? query.OrderByDescending(x => x.Email).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)
                : query.OrderBy(x => x.Email).ThenBy(x => x.FirstName).ThenBy(x => x.LastName),
            "mobilephone" => descending
                ? query.OrderByDescending(x => x.MobilePhone).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)
                : query.OrderBy(x => x.MobilePhone).ThenBy(x => x.FirstName).ThenBy(x => x.LastName),
            "createdat" => descending
                ? query.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)
                : query.OrderBy(x => x.CreatedAt).ThenBy(x => x.FirstName).ThenBy(x => x.LastName),
            "customertype" => descending
                ? query.OrderByDescending(x => x.CustomerType).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)
                : query.OrderBy(x => x.CustomerType).ThenBy(x => x.FirstName).ThenBy(x => x.LastName),
            "isactive" => descending
                ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.FirstName).ThenBy(x => x.LastName),
            "isvip" => descending
                ? query.OrderByDescending(x => x.IsVip).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)
                : query.OrderBy(x => x.IsVip).ThenBy(x => x.FirstName).ThenBy(x => x.LastName),
            "company" or "companyname" => descending
                ? query.OrderByDescending(x => x.Company == null ? null : x.Company.Name).ThenBy(x => x.FirstName).ThenBy(x => x.LastName)
                : query.OrderBy(x => x.Company == null ? null : x.Company.Name).ThenBy(x => x.FirstName).ThenBy(x => x.LastName),
            _ => descending
                ? query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName)
                : query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
        };
    }
}
