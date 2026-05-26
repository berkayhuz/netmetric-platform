// <copyright file="OpportunityStageTransitionResultDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.PipelineManagement.Contracts.DTOs;

public sealed record OpportunityStageTransitionResultDto(Guid OpportunityId, OpportunityStageType PreviousStage, OpportunityStageType CurrentStage, OpportunityStatusType Status, Guid? LostReasonId, string? LostNote, string? RowVersion);
