// <copyright file="MarketingAutomationDtos.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.MarketingAutomation.Contracts.DTOs;

public sealed class MarketingAutomationSummaryDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required bool IsActive { get; init; }
}

public sealed record MarketingCampaignDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Status,
    Guid? SegmentId,
    Guid? EmailTemplateId,
    DateTime? ScheduledAtUtc,
    decimal BudgetAmount,
    decimal ExpectedRevenueAmount,
    string ApprovalStatus,
    string UtmCampaign,
    bool IsActive);

public sealed record MarketingSegmentDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string SegmentType,
    string CriteriaJson,
    bool IsSuppression,
    int LastEvaluatedCount,
    DateTime? LastEvaluatedAtUtc,
    bool IsActive);

public sealed record MarketingSegmentEvaluationDto(Guid SegmentId, int MatchedCount, IReadOnlyCollection<MarketingAudienceMemberDto> Members);

public sealed record MarketingAudienceMemberDto(string EmailHash, string? EmailAddress, Guid? ContactId, string Reason);

public sealed record MarketingEmailTemplateDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Subject,
    string FromName,
    string FromEmail,
    string HtmlBody,
    string TextBody,
    string DeliverabilityStatus);

public sealed record MarketingTemplatePreviewDto(string Subject, string HtmlBody, string TextBody);

public sealed record MarketingJourneyDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Status,
    Guid? EntrySegmentId,
    string StepDefinitionJson,
    DateTime? StartedAtUtc,
    DateTime? PausedAtUtc);

public sealed record MarketingSuppressionDto(Guid Id, string EmailHash, string EmailAddress, string Reason, string Source, DateTime SuppressedAtUtc);

public sealed record MarketingConsentDto(Guid Id, string EmailHash, string EmailAddress, string Status, string Source, bool DoubleOptInRequired, DateTime? GrantedAtUtc, DateTime? RevokedAtUtc);

public sealed record MarketingRoiDto(Guid CampaignId, int SentCount, int SuppressedCount, decimal RevenueAmount, decimal CostAmount, decimal RoiPercent, DateTime CalculatedAtUtc);

public sealed record MarketingWorkerStatusDto(
    bool EngineEnabled,
    bool WorkerEnabled,
    int ScheduledCampaigns,
    int DueDeliveries,
    int DueJourneySteps,
    int FailedDeliveries,
    DateTime CheckedAtUtc);

public sealed record MarketingCommandResult(Guid Id);
