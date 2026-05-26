// <copyright file="MarketingAutomationAbstractions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.MarketingAutomation.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.MarketingAutomation.Application.Abstractions;

public interface IMarketingAutomationService
{
    Task<PagedResult<MarketingCampaignDto>> ListCampaignsAsync(Guid tenantId, int page, int pageSize, string? status, CancellationToken cancellationToken);
    Task<MarketingCampaignDto?> GetCampaignAsync(Guid tenantId, Guid campaignId, CancellationToken cancellationToken);
    Task<MarketingCampaignDto> CreateCampaignAsync(Guid tenantId, MarketingCampaignUpsertRequest request, CancellationToken cancellationToken);
    Task<MarketingCampaignDto> UpdateCampaignAsync(Guid tenantId, Guid campaignId, MarketingCampaignUpsertRequest request, CancellationToken cancellationToken);
    Task ScheduleCampaignAsync(Guid tenantId, Guid campaignId, DateTime scheduledAtUtc, CancellationToken cancellationToken);
    Task PauseCampaignAsync(Guid tenantId, Guid campaignId, CancellationToken cancellationToken);
    Task ResumeCampaignAsync(Guid tenantId, Guid campaignId, DateTime scheduledAtUtc, CancellationToken cancellationToken);
    Task CancelCampaignAsync(Guid tenantId, Guid campaignId, CancellationToken cancellationToken);
    Task<PagedResult<MarketingSegmentDto>> ListSegmentsAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken);
    Task<MarketingSegmentDto?> GetSegmentAsync(Guid tenantId, Guid segmentId, CancellationToken cancellationToken);
    Task<MarketingSegmentEvaluationDto> EvaluateSegmentAsync(Guid tenantId, Guid segmentId, IReadOnlyCollection<MarketingAudienceMemberInput> audience, CancellationToken cancellationToken);
    Task<PagedResult<MarketingEmailTemplateDto>> ListTemplatesAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken);
    Task<MarketingEmailTemplateDto?> GetTemplateAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken);
    Task<MarketingTemplatePreviewDto> PreviewTemplateAsync(Guid tenantId, Guid templateId, string payloadJson, CancellationToken cancellationToken);
    Task<PagedResult<MarketingJourneyDto>> ListJourneysAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken);
    Task<MarketingJourneyDto?> GetJourneyAsync(Guid tenantId, Guid journeyId, CancellationToken cancellationToken);
    Task StartJourneyAsync(Guid tenantId, Guid journeyId, IReadOnlyCollection<MarketingAudienceMemberInput> audience, CancellationToken cancellationToken);
    Task PauseJourneyAsync(Guid tenantId, Guid journeyId, CancellationToken cancellationToken);
    Task<PagedResult<MarketingSuppressionDto>> ListSuppressionsAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken);
    Task<MarketingSuppressionDto> AddSuppressionAsync(Guid tenantId, MarketingSuppressionRequest request, CancellationToken cancellationToken);
    Task<MarketingConsentDto> UpsertConsentAsync(Guid tenantId, MarketingConsentRequest request, CancellationToken cancellationToken);
    Task<MarketingConsentDto> UnsubscribeAsync(Guid tenantId, MarketingUnsubscribeRequest request, CancellationToken cancellationToken);
    Task<MarketingRoiDto> GetRoiAsync(Guid tenantId, Guid campaignId, CancellationToken cancellationToken);
    Task<MarketingWorkerStatusDto> GetWorkerStatusAsync(Guid tenantId, CancellationToken cancellationToken);
}

public interface IMarketingSegmentEvaluator
{
    MarketingSegmentEvaluationDto Evaluate(Guid segmentId, string criteriaJson, IReadOnlyCollection<MarketingAudienceMemberInput> audience);
}

public interface IMarketingConsentEnforcementService
{
    Task<MarketingEligibilityResult> CheckAsync(Guid tenantId, string emailAddress, Guid? campaignId, CancellationToken cancellationToken);
}

public interface IMarketingConsentTokenService
{
    string Issue(MarketingConsentTokenIssueRequest request);

    Task<MarketingConsentTokenValidationResult> ValidateAndConsumeAsync(
        Guid tenantId,
        string token,
        string expectedPurpose,
        CancellationToken cancellationToken);
}

public interface IMarketingTemplateRenderer
{
    MarketingTemplatePreviewDto Render(string subject, string htmlBody, string textBody, string payloadJson);
}

public interface IMarketingCampaignScheduler
{
    Task<int> ScheduleDueCampaignsAsync(CancellationToken cancellationToken);
    Task<int> ProcessDueDeliveriesAsync(CancellationToken cancellationToken);
}

public interface IMarketingJourneyExecutor
{
    Task<int> ProcessDueStepsAsync(CancellationToken cancellationToken);
}

public interface IMarketingPermissionGuard
{
    void Ensure(string permission);
}

public interface IMarketingEmailDeliveryProvider
{
    Task<MarketingEmailDeliveryProviderResult> SendAsync(MarketingEmailDeliveryProviderRequest request, CancellationToken cancellationToken);
}

public sealed record MarketingCampaignUpsertRequest(
    string Code,
    string Name,
    string? Description,
    Guid? SegmentId,
    Guid? EmailTemplateId,
    decimal BudgetAmount,
    decimal ExpectedRevenueAmount,
    int FrequencyCapPerContact,
    int FrequencyCapWindowDays,
    string? UtmCampaign);

public sealed record MarketingAudienceMemberInput(string EmailAddress, Guid? ContactId, string PayloadJson);

public sealed record MarketingSuppressionRequest(string EmailAddress, string Reason, string Source);

public sealed record MarketingConsentRequest(string EmailAddress, string Status, string Source, bool DoubleOptInRequired);

public sealed record MarketingUnsubscribeRequest(string EmailAddress, string Source);

public sealed record MarketingConsentTokenIssueRequest(
    Guid TenantId,
    string EmailAddress,
    string Purpose,
    string Source,
    string? Status = null,
    bool DoubleOptInRequired = false,
    DateTimeOffset? ExpiresAtUtc = null);

public sealed record MarketingConsentTokenValidationResult(
    bool IsValid,
    string? Reason,
    string? EmailAddress,
    string? Source,
    string? Status,
    bool DoubleOptInRequired)
{
    public static MarketingConsentTokenValidationResult Invalid(string reason) =>
        new(false, reason, null, null, null, false);

    public static MarketingConsentTokenValidationResult Valid(
        string emailAddress,
        string source,
        string? status,
        bool doubleOptInRequired) =>
        new(true, null, emailAddress, source, status, doubleOptInRequired);
}

public sealed record MarketingEligibilityResult(bool Allowed, string Reason, string ConsentStatus, bool Suppressed);

public sealed record MarketingEmailDeliveryProviderRequest(Guid TenantId, Guid CampaignId, string EmailHash, string CorrelationId);

public sealed record MarketingEmailDeliveryProviderResult(bool Accepted, string ProviderMessageId);
