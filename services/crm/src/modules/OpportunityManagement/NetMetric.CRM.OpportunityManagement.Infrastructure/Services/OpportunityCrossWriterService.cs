// <copyright file="OpportunityCrossWriterService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Contracts.Integration;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.Exceptions;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Services;

public sealed class OpportunityCrossWriterService(
    OpportunityManagementDbContext dbContext,
    IOpportunityManagementOutbox outbox) : IOpportunityCrossWriterService
{
    public async Task<Guid> CreateAsync(OpportunityCrossWriterCreateRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var actor = request.Actor;

        var opportunity = new Opportunity
        {
            TenantId = request.TenantId,
            OpportunityCode = request.OpportunityCode.Trim(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            EstimatedAmount = request.EstimatedAmount,
            ExpectedRevenue = request.ExpectedRevenue,
            Probability = request.Probability,
            EstimatedCloseDate = request.EstimatedCloseDate,
            Stage = request.Stage,
            Status = request.Status,
            Priority = request.Priority,
            LeadId = request.LeadId,
            CustomerId = request.CustomerId,
            OwnerUserId = request.OwnerUserId,
            PipelineId = request.PipelineId,
            PipelineStageId = request.PipelineStageId,
            ForecastCategory = request.ForecastCategory ?? NetMetric.CRM.Types.ForecastCategory.Omitted,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actor,
            UpdatedBy = actor,
            RowVersion = [1]
        };

        opportunity.SetNotes(request.Notes);

        await dbContext.Opportunities.AddAsync(opportunity, cancellationToken);

        await dbContext.OpportunityStageHistories.AddAsync(new OpportunityStageHistory
        {
            TenantId = request.TenantId,
            OpportunityId = opportunity.Id,
            OldStage = opportunity.Stage,
            NewStage = opportunity.Stage,
            ChangedAt = now,
            ChangedByUserId = null,
            Note = "Opportunity created from cross-module conversion.",
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = actor,
            UpdatedBy = actor
        }, cancellationToken);

        await outbox.EnqueueOpportunityCreatedAsync(opportunity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return opportunity.Id;
    }

    public async Task<OpportunityCrossWriterStageChangeResult> ChangeStageAsync(OpportunityCrossWriterStageChangeRequest request, CancellationToken cancellationToken = default)
    {
        var opportunity = await dbContext.Opportunities.FirstOrDefaultAsync(
                x => x.Id == request.OpportunityId && x.TenantId == request.TenantId,
                cancellationToken)
            ?? throw new NotFoundAppException("Opportunity not found.");

        if (!string.IsNullOrWhiteSpace(request.RowVersion))
        {
            opportunity.RowVersion = Convert.FromBase64String(request.RowVersion);
        }

        var previousStage = opportunity.Stage;
        var now = DateTime.UtcNow;

        opportunity.Stage = request.NewStage;
        opportunity.Status = request.Status;
        opportunity.LostReasonId = request.NewStage == NetMetric.CRM.Types.OpportunityStageType.Lost ? request.LostReasonId : null;
        opportunity.LostNote = request.NewStage == NetMetric.CRM.Types.OpportunityStageType.Lost ? request.LostNote?.Trim() : null;
        opportunity.PipelineId = request.PipelineId;
        opportunity.PipelineStageId = request.PipelineStageId;
        if (request.Probability.HasValue)
        {
            opportunity.Probability = request.Probability.Value;
        }

        opportunity.ForecastCategory = request.ForecastCategory ?? opportunity.ForecastCategory;
        opportunity.UpdatedAt = now;
        opportunity.UpdatedBy = request.Actor;

        await dbContext.OpportunityStageHistories.AddAsync(new OpportunityStageHistory
        {
            TenantId = request.TenantId,
            OpportunityId = opportunity.Id,
            OldStage = previousStage,
            NewStage = request.NewStage,
            ChangedAt = now,
            ChangedByUserId = request.ChangedByUserId,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = request.Actor,
            UpdatedBy = request.Actor
        }, cancellationToken);

        await outbox.EnqueueOpportunityUpdatedAsync(opportunity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new OpportunityCrossWriterStageChangeResult(
            opportunity.Id,
            previousStage,
            opportunity.Stage,
            opportunity.Status,
            opportunity.LostReasonId,
            opportunity.LostNote,
            Convert.ToBase64String(opportunity.RowVersion));
    }
}
