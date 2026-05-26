// <copyright file="PipelineColumnDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.OpportunityManagement.Contracts.DTOs;

public sealed record PipelineColumnDto(OpportunityStageType Stage, int Count, decimal TotalEstimatedAmount, IReadOnlyList<OpportunityListItemDto> Items);
