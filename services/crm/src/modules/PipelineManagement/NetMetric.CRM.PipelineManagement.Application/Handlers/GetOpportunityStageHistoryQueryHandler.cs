// <copyright file="GetOpportunityStageHistoryQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Queries;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class GetOpportunityStageHistoryQueryHandler(
    IPipelineManagementDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetOpportunityStageHistoryQuery, IReadOnlyList<OpportunityStageHistoryDto>>
{
    public async Task<IReadOnlyList<OpportunityStageHistoryDto>> Handle(GetOpportunityStageHistoryQuery request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.EnsureTenant();

        var opportunityExists = await dbContext.Opportunities.AnyAsync(
            x => x.Id == request.OpportunityId && x.TenantId == tenantId,
            cancellationToken);

        if (!opportunityExists)
            throw new NotFoundAppException("Opportunity not found.");

        return await dbContext.OpportunityStageHistories
            .Where(x => x.OpportunityId == request.OpportunityId && x.TenantId == tenantId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);
    }
}
