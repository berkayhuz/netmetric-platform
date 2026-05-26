// <copyright file="ChangeOpportunityStageRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.PipelineManagement.Contracts.Requests;

public sealed record ChangeOpportunityStageRequest(
    OpportunityStageType NewStage,
    Guid? NewPipelineStageId,
    string? Note,
    Guid? LostReasonId,
    string? LostNote,
    string? RowVersion);
