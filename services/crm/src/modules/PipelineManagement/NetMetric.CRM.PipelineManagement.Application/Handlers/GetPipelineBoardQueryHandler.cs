// <copyright file="GetPipelineBoardQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Queries;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CRM.Types;
using NetMetric.Exceptions;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class GetPipelineBoardQueryHandler(
    IPipelineManagementDbContext context)
    : IRequestHandler<GetPipelineBoardQuery, PipelineBoardDto>
{
    public async Task<PipelineBoardDto> Handle(GetPipelineBoardQuery request, CancellationToken cancellationToken)
    {
        var pipeline = await context.Pipelines
            .Include(p => p.Stages)
            .FirstOrDefaultAsync(p => p.Id == request.PipelineId, cancellationToken)
            ?? throw new NotFoundAppException("Pipeline not found.");

        var opportunitiesQuery = context.Opportunities
            .Where(o => o.PipelineId == request.PipelineId && o.Status == OpportunityStatusType.Open);

        if (request.OwnerUserId.HasValue)
        {
            opportunitiesQuery = opportunitiesQuery.Where(o => o.OwnerUserId == request.OwnerUserId);
        }

        var opportunities = await opportunitiesQuery.ToListAsync(cancellationToken);

        var columns = new List<PipelineBoardColumnDto>();
        foreach (var stage in pipeline.Stages.OrderBy(s => s.DisplayOrder))
        {
            var stageOpps = opportunities
                .Where(o => o.PipelineStageId == stage.Id)
                .Select(o => new OpportunitySummaryDto(
                    o.Id,
                    o.Name,
                    o.OpportunityCode,
                    o.EstimatedAmount,
                    null, // Customer name would require a join/include
                    o.EstimatedCloseDate,
                    (DateTime.UtcNow - (o.UpdatedAt ?? o.CreatedAt)).TotalDays > 30,
                    0 // Warning count TODO
                ))
                .ToList();

            columns.Add(new PipelineBoardColumnDto(
                stage.Id,
                stage.Name,
                stage.Probability,
                stageOpps.Count,
                stageOpps.Sum(o => o.Amount),
                stageOpps));
        }

        return new PipelineBoardDto(pipeline.Id, pipeline.Name, columns);
    }
}
