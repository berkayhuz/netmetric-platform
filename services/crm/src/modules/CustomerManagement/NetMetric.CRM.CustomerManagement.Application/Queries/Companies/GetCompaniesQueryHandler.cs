// <copyright file="GetCompaniesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.CustomerManagement.Application.Queries.Companies;

public sealed class GetCompaniesQueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService)
    : IRequestHandler<GetCompaniesQuery, PagedResult<CompanyListItemDto>>
{
    public async Task<PagedResult<CompanyListItemDto>> Handle(
        GetCompaniesQuery request,
        CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.CompaniesResource);
        var canSeeContactData = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.CompaniesResource, "contactData", scope.Permissions)
            .Visibility == FieldVisibility.Visible;
        var canSeeTaxData = fieldAuthorizationService
            .Decide(CrmAuthorizationCatalog.CompaniesResource, "taxData", scope.Permissions)
            .Visibility == FieldVisibility.Visible;
        var pageNumber = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize > 200 ? 200 : request.PageSize;

        var query = dbContext.Companies
            .AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Name, search) ||
                (canSeeContactData && x.Email != null && EF.Functions.Like(x.Email, search)) ||
                (canSeeContactData && x.Phone != null && EF.Functions.Like(x.Phone, search)) ||
                (canSeeTaxData && x.TaxNumber != null && EF.Functions.Like(x.TaxNumber, search)) ||
                (x.Sector != null && EF.Functions.Like(x.Sector, search)));
        }

        if (request.CompanyType.HasValue)
            query = query.Where(x => x.CompanyType == request.CompanyType.Value);

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var page = await query
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Email,
                x.Phone,
                x.CompanyType,
                x.Sector,
                x.IsActive,
                ContactCount = x.Contacts.Count,
                x.RowVersion,
                x.LogoUrl
            })
            .ToListAsync(cancellationToken);

        var items = page
            .Select(x => new CompanyListItemDto(
                x.Id,
                x.Name,
                canSeeContactData ? x.Email : null,
                canSeeContactData ? x.Phone : null,
                x.CompanyType,
                x.Sector,
                x.IsActive,
                x.ContactCount,
                Convert.ToBase64String(x.RowVersion),
                x.LogoUrl))
            .ToList();

        return new PagedResult<CompanyListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
