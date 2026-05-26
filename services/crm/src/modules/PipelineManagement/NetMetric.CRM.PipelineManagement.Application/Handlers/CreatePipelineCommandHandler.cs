// <copyright file="CreatePipelineCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Integration;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CRM.PipelineManagement.Domain.Entities;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class CreatePipelineCommandHandler(
    IPipelineManagementDbContext context,
    IPipelineManagementOutbox outbox)
    : IRequestHandler<CreatePipelineCommand, PipelineDto>
{
    public async Task<PipelineDto> Handle(CreatePipelineCommand request, CancellationToken cancellationToken)
    {
        var pipeline = new Pipeline
        {
            Name = request.Name,
            Description = request.Description,
            IsDefault = request.IsDefault,
            DisplayOrder = request.DisplayOrder
        };

        foreach (var stageRequest in request.Stages)
        {
            pipeline.Stages.Add(new PipelineStage
            {
                Name = stageRequest.Name,
                Description = stageRequest.Description,
                DisplayOrder = stageRequest.DisplayOrder,
                Probability = stageRequest.Probability,
                IsWinStage = stageRequest.IsWinStage,
                IsLostStage = stageRequest.IsLostStage
            });
        }

        context.Pipelines.Add(pipeline);
        await outbox.EnqueuePipelineCreatedAsync(pipeline, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return pipeline.ToDto();
    }
}
