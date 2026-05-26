// <copyright file="GetPipelineBoardQueryHandler.cs" company="NetMetric">
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
using NetMetric.CRM.Types;

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Pipeline.Queries.GetPipelineBoard;

public sealed class GetPipelineBoardQueryHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetPipelineBoardQuery, IReadOnlyList<PipelineColumnDto>>
{
    public async Task<IReadOnlyList<PipelineColumnDto>> Handle(GetPipelineBoardQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.OpportunitiesResource);
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var baseQuery = dbContext.Opportunities.AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .Where(x => x.Status != OpportunityStatusType.Cancelled);
        if (request.OwnerUserId.HasValue) baseQuery = baseQuery.Where(x => x.OwnerUserId == request.OwnerUserId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            baseQuery = baseQuery.Where(x => x.Name.Contains(search) || x.OpportunityCode.Contains(search));
        }

        var stages = new[] { OpportunityStageType.Prospecting, OpportunityStageType.Qualification, OpportunityStageType.NeedsAnalysis, OpportunityStageType.Proposal, OpportunityStageType.Negotiation, OpportunityStageType.Won, OpportunityStageType.Lost };
        var columns = new List<PipelineColumnDto>();
        foreach (var stage in stages)
        {
            var stageQuery = baseQuery.Where(x => x.Stage == stage);
            var count = await stageQuery.CountAsync(cancellationToken);
            var total = canSeeFinancialData
                ? await stageQuery.SumAsync(x => (decimal?)x.EstimatedAmount, cancellationToken) ?? 0m
                : 0m;
            var items = await stageQuery.OrderByDescending(x => x.ExpectedRevenue ?? x.EstimatedAmount).Take(request.MaxItemsPerStage < 1 ? 25 : request.MaxItemsPerStage).ToListAsync(cancellationToken);
            columns.Add(new PipelineColumnDto(stage, count, total, items.Select(x => x.ToListItemDto(canSeeFinancialData)).ToList()));
        }

        return columns;
    }
}
