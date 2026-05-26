// <copyright file="GetQuotesByOpportunityQueryHandler.cs" company="NetMetric">
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

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Quotes.Queries.GetQuotesByOpportunity;

public sealed class GetQuotesByOpportunityQueryHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetQuotesByOpportunityQuery, IReadOnlyList<QuoteDetailDto>>
{
    public async Task<IReadOnlyList<QuoteDetailDto>> Handle(GetQuotesByOpportunityQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.OpportunitiesResource);
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var allowedOpportunityExists = await dbContext.Opportunities.AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .AnyAsync(x => x.Id == request.OpportunityId, cancellationToken);
        if (!allowedOpportunityExists)
        {
            return [];
        }

        var quotes = await dbContext.Quotes.AsNoTracking().Include(x => x.Items).Where(x => x.TenantId == scope.TenantId && x.OpportunityId == request.OpportunityId).OrderByDescending(x => x.QuoteDate).ToListAsync(cancellationToken);
        return quotes.Select(x => x.ToDto(canSeeFinancialData, canSeeInternalNotes)).ToList();
    }
}
