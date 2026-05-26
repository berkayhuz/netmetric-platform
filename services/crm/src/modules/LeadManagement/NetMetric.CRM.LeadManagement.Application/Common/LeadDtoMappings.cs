// <copyright file="LeadDtoMappings.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.LeadManagement.Contracts.DTOs;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.LeadManagement.Application.Common;

public static class LeadDtoMappings
{
    public static LeadScoreDto ToDto(this LeadScore score)
        => new(score.Id, score.Score, score.ScoreReason, score.CalculatedAt);

    public static LeadOwnershipHistoryDto ToDto(this LeadOwnershipHistory history)
        => new(history.Id, history.PreviousOwnerId, history.NewOwnerId, history.AssignmentReason, history.AssignmentRuleId, history.AssignedAt, history.AssignedByUserId);

    public static LeadDetailDto ToDetailDto(
        this Lead lead,
        IReadOnlyList<LeadScore> scores,
        bool includeContactData = true,
        bool includeFinancialData = true,
        bool includeInternalNotes = true)
        => new(
            lead.Id,
            lead.LeadCode,
            lead.FullName,
            lead.CompanyName,
            includeContactData ? lead.Email : null,
            includeContactData ? lead.Phone : null,
            lead.JobTitle,
            lead.Description,
            includeFinancialData ? lead.EstimatedBudget : null,
            lead.NextContactDate,
            lead.Source,
            lead.Status,
            lead.Priority,
            lead.CompanyId,
            lead.OwnerUserId,
            lead.ConvertedCustomerId,
            includeInternalNotes ? lead.Notes : null,
            lead.TotalScore,
            lead.FitScore,
            lead.Grade,
            lead.QualificationFramework,
            lead.QualificationData,
            lead.SlaTargetTime,
            lead.FirstContactTime,
            lead.SlaBreached,
            lead.UtmSource,
            lead.UtmMedium,
            lead.UtmCampaign,
            lead.ReferrerUrl,
            lead.IsActive,
            scores.Select(ToDto).ToList(),
            lead.OwnershipHistories.Select(ToDto).ToList(),
            Convert.ToBase64String(lead.RowVersion));
}
