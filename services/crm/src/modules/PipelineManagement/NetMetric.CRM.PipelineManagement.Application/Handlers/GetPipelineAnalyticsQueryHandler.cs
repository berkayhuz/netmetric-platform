// <copyright file="GetPipelineAnalyticsQueryHandler.cs" company="NetMetric">
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

public sealed class GetPipelineAnalyticsQueryHandler(
    IPipelineManagementDbContext context)
    : IRequestHandler<GetPipelineAnalyticsQuery, PipelineAnalyticsDto>
{
    public async Task<PipelineAnalyticsDto> Handle(GetPipelineAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var pipeline = await context.Pipelines
            .Include(p => p.Stages)
            .FirstOrDefaultAsync(p => p.Id == request.PipelineId, cancellationToken)
            ?? throw new NotFoundAppException("Pipeline not found.");

        var opportunities = await context.Opportunities
            .Where(o => o.PipelineId == request.PipelineId && o.Status == OpportunityStatusType.Open)
            .ToListAsync(cancellationToken);

        // 1. Aging Calculation
        var stageAging = new List<StageAgingDto>();
        foreach (var stage in pipeline.Stages)
        {
            var stageOpps = opportunities.Where(o => o.PipelineStageId == stage.Id).ToList();
            var avgDays = stageOpps.Count != 0
                ? stageOpps.Average(o => (DateTime.UtcNow - o.CreatedAt).TotalDays)
                : 0;

            var staleCount = stageOpps.Count(o => (DateTime.UtcNow - (o.UpdatedAt ?? o.CreatedAt)).TotalDays > 30);

            stageAging.Add(new StageAgingDto(
                stage.Id,
                stage.Name,
                stageOpps.Count,
                avgDays,
                staleCount));
        }

        // 2. Velocity Calculation (Average days to Won)
        var wonOpportunities = await context.Opportunities
            .Where(o => o.PipelineId == request.PipelineId && o.Status == OpportunityStatusType.Won)
            .OrderByDescending(o => o.UpdatedAt)
            .Take(50) // Last 50 won deals
            .ToListAsync(cancellationToken);

        var velocity = wonOpportunities.Count != 0
            ? wonOpportunities.Average(o => (o.UpdatedAt!.Value - o.CreatedAt).TotalDays)
            : 0;

        // 3. Health Score (Heuristic)
        decimal healthScore = 100;
        if (opportunities.Count != 0)
        {
            var staleRatio = (decimal)stageAging.Sum(s => s.StaleCount) / opportunities.Count;
            healthScore -= staleRatio * 50; // Deduct up to 50 points for staleness

            var pastCloseDateCount = opportunities.Count(o => o.EstimatedCloseDate < DateTime.UtcNow);
            var pastCloseRatio = (decimal)pastCloseDateCount / opportunities.Count;
            healthScore -= pastCloseRatio * 30; // Deduct up to 30 points for past close dates
        }
        healthScore = Math.Max(0, healthScore);

        // 4. Coverage Ratio (Assuming we have a quota or target, but for now just Pipeline Value / Target)
        // For demonstration, we'll just return a mock ratio or skip.
        decimal coverageRatio = 3.5m; // Total Pipeline / Quota (Mocked)

        return new PipelineAnalyticsDto(
            pipeline.Id,
            healthScore,
            (decimal)velocity,
            coverageRatio,
            opportunities.Count,
            opportunities.Sum(o => o.EstimatedAmount),
            stageAging);
    }
}
