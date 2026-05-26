// <copyright file="GetPipelinesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Queries;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class GetPipelinesQueryHandler(
    IPipelineManagementDbContext context)
    : IRequestHandler<GetPipelinesQuery, List<PipelineSummaryDto>>
{
    public async Task<List<PipelineSummaryDto>> Handle(GetPipelinesQuery request, CancellationToken cancellationToken)
    {
        return await context.Pipelines
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new PipelineSummaryDto(x.Id, x.Name, x.Stages.Count, x.IsDefault))
            .ToListAsync(cancellationToken);
    }
}
