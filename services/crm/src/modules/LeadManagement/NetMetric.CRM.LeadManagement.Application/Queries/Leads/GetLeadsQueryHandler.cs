// <copyright file="GetLeadsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.LeadManagement.Application.Queries.Leads;

public sealed class GetLeadsQueryHandler(
    ILeadManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService)
    : IRequestHandler<GetLeadsQuery, PagedResult<LeadListItemDto>>
{
    public async Task<PagedResult<LeadListItemDto>> Handle(GetLeadsQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.LeadsResource);
        var canSeeContactData = fieldAuthorizationService.Decide(scope.Resource, "contactData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var pageRequest = PageRequest.Normalize(request.Page, request.PageSize);

        var query = dbContext.Leads
            .AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.FullName, search) ||
                (x.CompanyName != null && EF.Functions.Like(x.CompanyName, search)) ||
                (canSeeContactData && x.Email != null && EF.Functions.Like(x.Email, search)) ||
                (canSeeContactData && x.Phone != null && EF.Functions.Like(x.Phone, search)) ||
                EF.Functions.Like(x.LeadCode, search));
        }

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);

        if (request.Source.HasValue)
            query = query.Where(x => x.Source == request.Source.Value);

        if (request.OwnerUserId.HasValue)
            query = query.Where(x => x.OwnerUserId == request.OwnerUserId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var leads = await query
            .OrderBy(x => x.FullName)
            .Skip(pageRequest.Skip)
            .Take(pageRequest.Size)
            .ToListAsync(cancellationToken);

        var items = leads
            .Select(x => new LeadListItemDto(
                x.Id,
                x.LeadCode,
                x.FullName,
                x.CompanyName,
                canSeeContactData ? x.Email : null,
                canSeeContactData ? x.Phone : null,
                x.Status,
                x.Source,
                x.Priority,
                x.OwnerUserId,
                canSeeFinancialData ? x.EstimatedBudget : null,
                x.NextContactDate,
                x.TotalScore,
                x.Grade,
                x.QualificationFramework,
                x.SlaTargetTime,
                x.SlaBreached,
                x.IsActive,
                Convert.ToBase64String(x.RowVersion)))
            .ToList();

        return PagedResult<LeadListItemDto>.Create(items, totalCount, pageRequest);
    }
}
