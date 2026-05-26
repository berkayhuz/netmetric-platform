// <copyright file="GetLeadByIdQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.LeadManagement.Application.Common;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;

namespace NetMetric.CRM.LeadManagement.Application.Queries.Leads;

public sealed class GetLeadByIdQueryHandler(
    ILeadManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService)
    : IRequestHandler<GetLeadByIdQuery, LeadDetailDto?>
{
    public async Task<LeadDetailDto?> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.LeadsResource);
        var canSeeContactData = fieldAuthorizationService.Decide(scope.Resource, "contactData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;

        var lead = await dbContext.Leads
            .AsNoTracking()
            .Include(x => x.OwnershipHistories)
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .FirstOrDefaultAsync(x => x.Id == request.LeadId, cancellationToken);

        if (lead is null)
            return null;

        var scores = await dbContext.LeadScores
            .AsNoTracking()
            .Where(x => x.TenantId == scope.TenantId && x.LeadId == request.LeadId)
            .OrderByDescending(x => x.CalculatedAt)
            .ToListAsync(cancellationToken);

        return lead.ToDetailDto(scores, canSeeContactData, canSeeFinancialData, canSeeInternalNotes);
    }
}
