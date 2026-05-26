// <copyright file="UpdatePipelineCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Integration;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CRM.PipelineManagement.Domain.Entities;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class UpdatePipelineCommandHandler(
    IPipelineManagementDbContext context,
    ICurrentUserService currentUserService,
    IPipelineManagementOutbox outbox)
    : IRequestHandler<UpdatePipelineCommand, PipelineDto>
{
    public async Task<PipelineDto> Handle(UpdatePipelineCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();

        var pipeline = await context.Pipelines
            .Include(p => p.Stages)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId, cancellationToken)
            ?? throw new NotFoundAppException("Pipeline not found.");

        pipeline.RowVersion = Convert.FromBase64String(request.RowVersion);
        pipeline.Name = request.Name;
        pipeline.Description = request.Description;
        pipeline.IsDefault = request.IsDefault;
        pipeline.DisplayOrder = request.DisplayOrder;

        // Simple sync logic for stages (in production would be more sophisticated)
        var existingStageIds = request.Stages.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToList();

        // Remove deleted stages
        var stagesToRemove = pipeline.Stages.Where(s => !existingStageIds.Contains(s.Id)).ToList();
        foreach (var stage in stagesToRemove) pipeline.Stages.Remove(stage);

        // Update or Add stages
        foreach (var stageRequest in request.Stages)
        {
            if (stageRequest.Id.HasValue)
            {
                var stage = pipeline.Stages.FirstOrDefault(s => s.Id == stageRequest.Id.Value);
                if (stage != null)
                {
                    stage.Name = stageRequest.Name;
                    stage.Description = stageRequest.Description;
                    stage.DisplayOrder = stageRequest.DisplayOrder;
                    stage.Probability = stageRequest.Probability;
                    stage.IsWinStage = stageRequest.IsWinStage;
                    stage.IsLostStage = stageRequest.IsLostStage;
                }
            }
            else
            {
                pipeline.Stages.Add(new PipelineStage
                {
                    Name = stageRequest.Name,
                    Description = stageRequest.Description,
                    DisplayOrder = stageRequest.DisplayOrder,
                    Probability = stageRequest.Probability,
                    IsWinStage = stageRequest.IsWinStage,
                    IsLostStage = stageRequest.IsLostStage,
                    TenantId = tenantId
                });
            }
        }

        await outbox.EnqueuePipelineUpdatedAsync(pipeline, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return pipeline.ToDto();
    }
}
