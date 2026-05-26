// <copyright file="OpportunityListItemDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.OpportunityManagement.Contracts.DTOs;

public sealed record OpportunityListItemDto(
    Guid Id,
    string OpportunityCode,
    string Name,
    decimal? EstimatedAmount,
    decimal? ExpectedRevenue,
    decimal Probability,
    OpportunityStageType Stage,
    OpportunityStatusType Status,
    PriorityType Priority,
    DateTime? EstimatedCloseDate,
    Guid? LeadId,
    Guid? CustomerId,
    Guid? OwnerUserId,
    bool IsActive);
