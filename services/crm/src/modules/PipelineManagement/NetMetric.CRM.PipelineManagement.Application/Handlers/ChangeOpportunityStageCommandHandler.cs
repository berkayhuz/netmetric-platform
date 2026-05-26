// <copyright file="ChangeOpportunityStageCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Contracts.Integration;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class ChangeOpportunityStageCommandHandler(
    IPipelineManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IOpportunityCrossWriterService opportunityCrossWriterService,
    NetMetric.CRM.PipelineManagement.Application.Abstractions.Services.IPipelineValidationService validationService)
    : IRequestHandler<ChangeOpportunityStageCommand, OpportunityStageTransitionResultDto>
{
    public async Task<OpportunityStageTransitionResultDto> Handle(ChangeOpportunityStageCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.EnsureTenant();

        var opportunity = await dbContext.Opportunities.FirstOrDefaultAsync(
                x => x.Id == request.OpportunityId && x.TenantId == tenantId,
                cancellationToken)
            ?? throw new NotFoundAppException("Opportunity not found.");
        var previousStage = opportunity.Stage;

        if (!string.IsNullOrWhiteSpace(request.RowVersion))
            opportunity.RowVersion = Convert.FromBase64String(request.RowVersion);

        // Dynamic Stage logic
        if (request.NewPipelineStageId.HasValue)
        {
            var stage = await dbContext.PipelineStages
                .Include(s => s.Pipeline)
                .Include(s => s.RequiredFields)
                .Include(s => s.ExitCriteria)
                .FirstOrDefaultAsync(x => x.Id == request.NewPipelineStageId.Value && x.TenantId == tenantId, cancellationToken)
                ?? throw new NotFoundAppException("Pipeline stage not found.");

            if (opportunity.PipelineStageId == request.NewPipelineStageId)
                throw new ConflictAppException("Opportunity is already in the requested pipeline stage.");

            var validationResult = await validationService.ValidateStageTransitionAsync(opportunity, request.NewPipelineStageId.Value, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new BadRequestAppException($"Stage transition validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            opportunity.PipelineId = stage.PipelineId;
            opportunity.PipelineStageId = stage.Id;
            opportunity.Stage = request.NewStage; // Still keep enum for compatibility if needed
            opportunity.Probability = stage.Probability;
            opportunity.Status = stage.IsWinStage ? OpportunityStatusType.Won :
                                 stage.IsLostStage ? OpportunityStatusType.Lost :
                                 OpportunityStatusType.Open;

            opportunity.ForecastCategory = stage.ForecastCategory;
        }
        else
        {
            // Legacy Enum logic (Fallback)
            if (opportunity.Stage == request.NewStage)
                throw new ConflictAppException("Opportunity is already in the requested stage.");

            opportunity.Stage = request.NewStage;
            // Note: In a production-grade system, we'd want to find the default pipeline and matching stage
        }

        if (request.NewStage == OpportunityStageType.Lost && request.LostReasonId is not null)
        {
            var lostReasonExists = await dbContext.LostReasons.AnyAsync(
                x => x.Id == request.LostReasonId.Value && x.TenantId == tenantId,
                cancellationToken);
            if (!lostReasonExists)
                throw new NotFoundAppException("Lost reason not found.");
        }

        opportunity.LostReasonId = request.NewStage == OpportunityStageType.Lost ? request.LostReasonId : null;
        opportunity.LostNote = request.NewStage == OpportunityStageType.Lost ? request.LostNote?.Trim() : null;
        opportunity.UpdatedAt = DateTime.UtcNow;
        opportunity.UpdatedBy = currentUserService.UserName;

        await opportunityCrossWriterService.ChangeStageAsync(
            new OpportunityCrossWriterStageChangeRequest(
                tenantId,
                opportunity.Id,
                request.NewStage,
                opportunity.Status,
                request.NewStage == OpportunityStageType.Lost ? request.LostReasonId : null,
                request.NewStage == OpportunityStageType.Lost ? request.LostNote?.Trim() : null,
                request.Note,
                opportunity.PipelineId,
                opportunity.PipelineStageId,
                opportunity.Probability,
                opportunity.ForecastCategory,
                request.RowVersion,
                currentUserService.UserId,
                currentUserService.UserName),
            cancellationToken);

        await dbContext.OpportunityStageHistories.AddAsync(new OpportunityStageHistory
        {
            TenantId = tenantId,
            OpportunityId = opportunity.Id,
            OldStage = previousStage,
            NewStage = request.NewStage,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = currentUserService.UserId,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new OpportunityStageTransitionResultDto(
            opportunity.Id,
            previousStage,
            opportunity.Stage,
            opportunity.Status,
            opportunity.LostReasonId,
            opportunity.LostNote,
            Convert.ToBase64String(opportunity.RowVersion));
    }
}
