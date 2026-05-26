// <copyright file="GetPipelineByIdQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Queries;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.Exceptions;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class GetPipelineByIdQueryHandler(
    IPipelineManagementDbContext context)
    : IRequestHandler<GetPipelineByIdQuery, PipelineDto>
{
    public async Task<PipelineDto> Handle(GetPipelineByIdQuery request, CancellationToken cancellationToken)
    {
        var pipeline = await context.Pipelines
            .Include(p => p.Stages)
                .ThenInclude(s => s.RequiredFields)
            .Include(p => p.Stages)
                .ThenInclude(s => s.ExitCriteria)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundAppException("Pipeline not found.");

        return pipeline.ToDto();
    }
}
