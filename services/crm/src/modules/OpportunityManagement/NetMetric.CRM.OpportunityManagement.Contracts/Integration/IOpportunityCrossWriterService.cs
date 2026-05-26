// <copyright file="IOpportunityCrossWriterService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.OpportunityManagement.Contracts.Integration;

public interface IOpportunityCrossWriterService
{
    Task<Guid> CreateAsync(OpportunityCrossWriterCreateRequest request, CancellationToken cancellationToken = default);

    Task<OpportunityCrossWriterStageChangeResult> ChangeStageAsync(OpportunityCrossWriterStageChangeRequest request, CancellationToken cancellationToken = default);
}

public sealed record OpportunityCrossWriterCreateRequest(
    Guid TenantId,
    string OpportunityCode,
    string Name,
    string? Description,
    decimal EstimatedAmount,
    decimal? ExpectedRevenue,
    decimal Probability,
    DateTime? EstimatedCloseDate,
    OpportunityStageType Stage,
    OpportunityStatusType Status,
    PriorityType Priority,
    Guid? LeadId,
    Guid? CustomerId,
    Guid? OwnerUserId,
    string? Notes,
    Guid? PipelineId,
    Guid? PipelineStageId,
    ForecastCategory? ForecastCategory,
    string? Actor);

public sealed record OpportunityCrossWriterStageChangeRequest(
    Guid TenantId,
    Guid OpportunityId,
    OpportunityStageType NewStage,
    OpportunityStatusType Status,
    Guid? LostReasonId,
    string? LostNote,
    string? Note,
    Guid? PipelineId,
    Guid? PipelineStageId,
    decimal? Probability,
    ForecastCategory? ForecastCategory,
    string? RowVersion,
    Guid? ChangedByUserId,
    string? Actor);

public sealed record OpportunityCrossWriterStageChangeResult(
    Guid OpportunityId,
    OpportunityStageType PreviousStage,
    OpportunityStageType CurrentStage,
    OpportunityStatusType Status,
    Guid? LostReasonId,
    string? LostNote,
    string RowVersion);
