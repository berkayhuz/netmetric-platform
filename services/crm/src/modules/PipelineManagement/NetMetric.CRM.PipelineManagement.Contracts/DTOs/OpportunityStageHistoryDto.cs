// <copyright file="OpportunityStageHistoryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.PipelineManagement.Contracts.DTOs;

public sealed record OpportunityStageHistoryDto(Guid Id, Guid OpportunityId, OpportunityStageType OldStage, OpportunityStageType NewStage, DateTime ChangedAt, Guid? ChangedByUserId, string? Note);
