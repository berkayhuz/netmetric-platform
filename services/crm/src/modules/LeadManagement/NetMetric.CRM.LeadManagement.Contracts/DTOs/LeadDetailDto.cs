// <copyright file="LeadDetailDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.LeadManagement.Contracts.DTOs;

public sealed record LeadDetailDto(
    Guid Id,
    string LeadCode,
    string FullName,
    string? CompanyName,
    string? Email,
    string? Phone,
    string? JobTitle,
    string? Description,
    decimal? EstimatedBudget,
    DateTime? NextContactDate,
    LeadSourceType Source,
    LeadStatusType Status,
    PriorityType Priority,
    Guid? CompanyId,
    Guid? OwnerUserId,
    Guid? ConvertedCustomerId,
    string? Notes,
    // New Fields
    decimal TotalScore,
    decimal FitScore,
    LeadGradeType Grade,
    QualificationFrameworkType QualificationFramework,
    string? QualificationData,
    DateTime? SlaTargetTime,
    DateTime? FirstContactTime,
    bool SlaBreached,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? ReferrerUrl,
    bool IsActive,
    IReadOnlyList<LeadScoreDto> Scores,
    IReadOnlyList<LeadOwnershipHistoryDto> OwnershipHistories,
    string RowVersion);

public sealed record LeadOwnershipHistoryDto(
    Guid Id,
    Guid? PreviousOwnerId,
    Guid? NewOwnerId,
    string? AssignmentReason,
    string? AssignmentRuleId,
    DateTime AssignedAt,
    Guid? AssignedByUserId);
