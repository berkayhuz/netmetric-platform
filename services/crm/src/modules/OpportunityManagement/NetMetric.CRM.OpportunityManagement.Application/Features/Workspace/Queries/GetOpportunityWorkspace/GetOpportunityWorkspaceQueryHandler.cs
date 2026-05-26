// <copyright file="GetOpportunityWorkspaceQueryHandler.cs" company="NetMetric">
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

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Workspace.Queries.GetOpportunityWorkspace;

public sealed class GetOpportunityWorkspaceQueryHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetOpportunityWorkspaceQuery, OpportunityWorkspaceDto?>
{
    public async Task<OpportunityWorkspaceDto?> Handle(GetOpportunityWorkspaceQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.OpportunitiesResource);
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var opportunity = await dbContext.Opportunities.AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .FirstOrDefaultAsync(x => x.Id == request.OpportunityId, cancellationToken);
        if (opportunity is null) return null;

        var products = await dbContext.OpportunityProducts.AsNoTracking().Where(x => x.TenantId == scope.TenantId && x.OpportunityId == request.OpportunityId).ToListAsync(cancellationToken);
        var contacts = await dbContext.OpportunityContacts.AsNoTracking().Where(x => x.TenantId == scope.TenantId && x.OpportunityId == request.OpportunityId).ToListAsync(cancellationToken);
        var quotes = await dbContext.Quotes.AsNoTracking().Include(x => x.Items).Where(x => x.TenantId == scope.TenantId && x.OpportunityId == request.OpportunityId).ToListAsync(cancellationToken);
        var stageHistory = await dbContext.OpportunityStageHistories.AsNoTracking().Where(x => x.TenantId == scope.TenantId && x.OpportunityId == request.OpportunityId).ToListAsync(cancellationToken);
        var activityCount = await dbContext.Activities.AsNoTracking().CountAsync(x => x.TenantId == scope.TenantId && x.OpportunityId == request.OpportunityId, cancellationToken);
        return new OpportunityWorkspaceDto(opportunity.ToDetailDto(products, contacts, quotes, stageHistory, canSeeFinancialData, canSeeInternalNotes), canSeeFinancialData ? quotes.Sum(x => x.GrandTotal) : null, quotes.Count, activityCount, stageHistory.Count);
    }
}
