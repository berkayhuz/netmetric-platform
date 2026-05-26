// <copyright file="GetLeadWorkspaceQueryHandler.cs" company="NetMetric">
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
using NetMetric.CRM.Types;

namespace NetMetric.CRM.LeadManagement.Application.Features.Workspace.Queries.GetLeadWorkspace;

public sealed class GetLeadWorkspaceQueryHandler(
    ILeadManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService)
    : IRequestHandler<GetLeadWorkspaceQuery, LeadWorkspaceDto?>
{
    public async Task<LeadWorkspaceDto?> Handle(GetLeadWorkspaceQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.LeadsResource);
        var canSeeContactData = fieldAuthorizationService.Decide(scope.Resource, "contactData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;

        var lead = await dbContext.Leads
            .AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .FirstOrDefaultAsync(x => x.Id == request.LeadId, cancellationToken);

        if (lead is null)
            return null;

        var scores = await dbContext.LeadScores
            .AsNoTracking()
            .Where(x => x.TenantId == scope.TenantId && x.LeadId == request.LeadId)
            .OrderByDescending(x => x.CalculatedAt)
            .ToListAsync(cancellationToken);

        var openOpportunityCount = await dbContext.Opportunities
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == scope.TenantId
                     && x.LeadId == request.LeadId
                     && x.Status == OpportunityStatusType.Open,
                cancellationToken);

        var detail = lead.ToDetailDto(scores, canSeeContactData, canSeeFinancialData, canSeeInternalNotes);

        return new LeadWorkspaceDto(
            detail,
            scores.Count,
            scores.FirstOrDefault()?.Score,
            openOpportunityCount);
    }
}
