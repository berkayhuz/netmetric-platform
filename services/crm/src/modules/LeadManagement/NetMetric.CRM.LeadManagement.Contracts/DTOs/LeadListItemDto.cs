// <copyright file="LeadListItemDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.LeadManagement.Contracts.DTOs;

public sealed record LeadListItemDto(
    Guid Id,
    string LeadCode,
    string FullName,
    string? CompanyName,
    string? Email,
    string? Phone,
    LeadStatusType Status,
    LeadSourceType Source,
    PriorityType Priority,
    Guid? OwnerUserId,
    decimal? EstimatedBudget,
    DateTime? NextContactDate,
    decimal TotalScore,
    LeadGradeType Grade,
    QualificationFrameworkType QualificationFramework,
    DateTime? SlaTargetTime,
    bool SlaBreached,
    bool IsActive,
    string RowVersion);
