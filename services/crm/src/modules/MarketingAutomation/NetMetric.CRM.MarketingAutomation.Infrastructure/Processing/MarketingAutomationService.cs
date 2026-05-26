// <copyright file="MarketingAutomationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions;
using NetMetric.CRM.MarketingAutomation.Contracts.DTOs;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Attribution;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Campaigns;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Consents;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.EmailCampaigns;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.LeadNurturing;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Segments;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Suppression;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Persistence;
using NetMetric.Pagination;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Processing;

public sealed class MarketingAutomationService(
    MarketingAutomationDbContext dbContext,
    IMarketingSegmentEvaluator segmentEvaluator,
    IMarketingTemplateRenderer templateRenderer,
    IMarketingPermissionGuard permissionGuard) : IMarketingAutomationService
{
    public async Task<PagedResult<MarketingCampaignDto>> ListCampaignsAsync(Guid tenantId, int page, int pageSize, string? status, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        var pageRequest = PageRequest.Normalize(page, pageSize);
        var query = dbContext.Campaigns.Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip(pageRequest.Skip).Take(pageRequest.Size).Select(x => ToDto(x)).ToListAsync(cancellationToken);
        return PagedResult<MarketingCampaignDto>.Create(items, total, pageRequest);
    }

    public Task<MarketingCampaignDto?> GetCampaignAsync(Guid tenantId, Guid campaignId, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        return dbContext.Campaigns.Where(x => x.TenantId == tenantId && x.Id == campaignId).Select(x => ToDto(x)).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<MarketingCampaignDto> CreateCampaignAsync(Guid tenantId, MarketingCampaignUpsertRequest request, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.manage");
        var campaign = new Campaign(request.Code, request.Name, request.Description);
        campaign.TenantId = tenantId;
        campaign.Update(request.Name, request.Description, request.SegmentId, request.EmailTemplateId, request.BudgetAmount, request.ExpectedRevenueAmount, request.FrequencyCapPerContact, request.FrequencyCapWindowDays, request.UtmCampaign);
        dbContext.Campaigns.Add(campaign);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(campaign);
    }

    public async Task<MarketingCampaignDto> UpdateCampaignAsync(Guid tenantId, Guid campaignId, MarketingCampaignUpsertRequest request, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.manage");
        var campaign = await dbContext.Campaigns.FirstAsync(x => x.TenantId == tenantId && x.Id == campaignId, cancellationToken);
        campaign.Update(request.Name, request.Description, request.SegmentId, request.EmailTemplateId, request.BudgetAmount, request.ExpectedRevenueAmount, request.FrequencyCapPerContact, request.FrequencyCapWindowDays, request.UtmCampaign);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(campaign);
    }

    public async Task ScheduleCampaignAsync(Guid tenantId, Guid campaignId, DateTime scheduledAtUtc, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.manage");
        var campaign = await dbContext.Campaigns.FirstAsync(x => x.TenantId == tenantId && x.Id == campaignId, cancellationToken);
        campaign.Schedule(scheduledAtUtc);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PauseCampaignAsync(Guid tenantId, Guid campaignId, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.manage");
        var campaign = await dbContext.Campaigns.FirstAsync(x => x.TenantId == tenantId && x.Id == campaignId, cancellationToken);
        campaign.Pause(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ResumeCampaignAsync(Guid tenantId, Guid campaignId, DateTime scheduledAtUtc, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.manage");
        var campaign = await dbContext.Campaigns.FirstAsync(x => x.TenantId == tenantId && x.Id == campaignId, cancellationToken);
        campaign.Resume(scheduledAtUtc);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelCampaignAsync(Guid tenantId, Guid campaignId, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.manage");
        var campaign = await dbContext.Campaigns.FirstAsync(x => x.TenantId == tenantId && x.Id == campaignId, cancellationToken);
        campaign.Cancel(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<MarketingSegmentDto>> ListSegmentsAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        var pageRequest = PageRequest.Normalize(page, pageSize);
        var query = dbContext.Segments.Where(x => x.TenantId == tenantId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Name).Skip(pageRequest.Skip).Take(pageRequest.Size).Select(x => ToDto(x)).ToListAsync(cancellationToken);
        return PagedResult<MarketingSegmentDto>.Create(items, total, pageRequest);
    }

    public Task<MarketingSegmentDto?> GetSegmentAsync(Guid tenantId, Guid segmentId, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        return dbContext.Segments.Where(x => x.TenantId == tenantId && x.Id == segmentId).Select(x => ToDto(x)).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<MarketingSegmentEvaluationDto> EvaluateSegmentAsync(Guid tenantId, Guid segmentId, IReadOnlyCollection<MarketingAudienceMemberInput> audience, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        var segment = await dbContext.Segments.FirstAsync(x => x.TenantId == tenantId && x.Id == segmentId, cancellationToken);
        var result = segmentEvaluator.Evaluate(segment.Id, segment.CriteriaJson, audience);
        segment.MarkEvaluated(result.MatchedCount, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<PagedResult<MarketingEmailTemplateDto>> ListTemplatesAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        var pageRequest = PageRequest.Normalize(page, pageSize);
        var query = dbContext.EmailCampaigns.Where(x => x.TenantId == tenantId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Name).Skip(pageRequest.Skip).Take(pageRequest.Size).Select(x => ToDto(x)).ToListAsync(cancellationToken);
        return PagedResult<MarketingEmailTemplateDto>.Create(items, total, pageRequest);
    }

    public Task<MarketingEmailTemplateDto?> GetTemplateAsync(Guid tenantId, Guid templateId, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        return dbContext.EmailCampaigns.Where(x => x.TenantId == tenantId && x.Id == templateId).Select(x => ToDto(x)).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<MarketingTemplatePreviewDto> PreviewTemplateAsync(Guid tenantId, Guid templateId, string payloadJson, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        var template = await dbContext.EmailCampaigns.FirstAsync(x => x.TenantId == tenantId && x.Id == templateId, cancellationToken);
        template.MarkPreviewed(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return templateRenderer.Render(template.Subject, template.HtmlBody, template.TextBody, payloadJson);
    }

    public async Task<PagedResult<MarketingJourneyDto>> ListJourneysAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        var pageRequest = PageRequest.Normalize(page, pageSize);
        var query = dbContext.LeadNurturing.Where(x => x.TenantId == tenantId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.Name).Skip(pageRequest.Skip).Take(pageRequest.Size).Select(x => ToDto(x)).ToListAsync(cancellationToken);
        return PagedResult<MarketingJourneyDto>.Create(items, total, pageRequest);
    }

    public Task<MarketingJourneyDto?> GetJourneyAsync(Guid tenantId, Guid journeyId, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        return dbContext.LeadNurturing.Where(x => x.TenantId == tenantId && x.Id == journeyId).Select(x => ToDto(x)).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task StartJourneyAsync(Guid tenantId, Guid journeyId, IReadOnlyCollection<MarketingAudienceMemberInput> audience, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.manage");
        var journey = await dbContext.LeadNurturing.FirstAsync(x => x.TenantId == tenantId && x.Id == journeyId, cancellationToken);
        journey.Start(DateTime.UtcNow);
        foreach (var member in audience)
        {
            var emailHash = MarketingUtilities.HashEmail(member.EmailAddress);
            var key = $"{tenantId:N}:{journey.Id:N}:entry:{emailHash}";
            if (!await dbContext.JourneyStepExecutions.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.IdempotencyKey == key, cancellationToken))
            {
                dbContext.JourneyStepExecutions.Add(new(journey.Id, "entry", emailHash, DateTime.UtcNow, key));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PauseJourneyAsync(Guid tenantId, Guid journeyId, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.manage");
        var journey = await dbContext.LeadNurturing.FirstAsync(x => x.TenantId == tenantId && x.Id == journeyId, cancellationToken);
        journey.Pause(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<MarketingSuppressionDto>> ListSuppressionsAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        var pageRequest = PageRequest.Normalize(page, pageSize);
        var query = dbContext.SuppressionEntries.Where(x => x.TenantId == tenantId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.SuppressedAtUtc).Skip(pageRequest.Skip).Take(pageRequest.Size).Select(x => ToDto(x)).ToListAsync(cancellationToken);
        return PagedResult<MarketingSuppressionDto>.Create(items, total, pageRequest);
    }

    public async Task<MarketingSuppressionDto> AddSuppressionAsync(Guid tenantId, MarketingSuppressionRequest request, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.manage");
        var hash = MarketingUtilities.HashEmail(request.EmailAddress);
        var existing = await dbContext.SuppressionEntries.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EmailHash == hash, cancellationToken);
        if (existing is null)
        {
            existing = new SuppressionEntry(request.EmailAddress, hash, request.Reason, request.Source) { TenantId = tenantId };
            dbContext.SuppressionEntries.Add(existing);
        }
        else
        {
            existing.UpdateReason(request.Reason, request.Source);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(existing);
    }

    public async Task<MarketingConsentDto> UpsertConsentAsync(Guid tenantId, MarketingConsentRequest request, CancellationToken cancellationToken)
    {
        var hash = MarketingUtilities.HashEmail(request.EmailAddress);
        var existing = await dbContext.MarketingConsents.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EmailHash == hash, cancellationToken);
        if (existing is null)
        {
            existing = new MarketingConsent(request.EmailAddress, hash, request.Status, request.Source, request.DoubleOptInRequired) { TenantId = tenantId };
            dbContext.MarketingConsents.Add(existing);
        }
        else if (request.Status == MarketingConsentStatuses.Revoked)
        {
            existing.Revoke(request.Source);
        }
        else
        {
            existing.Grant(request.Source, request.DoubleOptInRequired);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(existing);
    }

    public async Task<MarketingConsentDto> UnsubscribeAsync(Guid tenantId, MarketingUnsubscribeRequest request, CancellationToken cancellationToken)
    {
        var consent = await UpsertConsentAsync(tenantId, new MarketingConsentRequest(request.EmailAddress, MarketingConsentStatuses.Revoked, request.Source, false), cancellationToken);
        var hash = MarketingUtilities.HashEmail(request.EmailAddress);
        var existing = await dbContext.SuppressionEntries.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EmailHash == hash, cancellationToken);
        if (existing is null)
        {
            dbContext.SuppressionEntries.Add(new SuppressionEntry(request.EmailAddress, hash, "unsubscribe", request.Source) { TenantId = tenantId });
        }
        else
        {
            existing.UpdateReason("unsubscribe", request.Source);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return consent;
    }

    public async Task<MarketingRoiDto> GetRoiAsync(Guid tenantId, Guid campaignId, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        await RecalculateRoiAsync(tenantId, campaignId, cancellationToken);
        return await dbContext.CampaignRoiProjections.Where(x => x.TenantId == tenantId && x.CampaignId == campaignId).Select(x => ToDto(x)).SingleAsync(cancellationToken);
    }

    public async Task<MarketingWorkerStatusDto> GetWorkerStatusAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        permissionGuard.Ensure("marketing.campaigns.read");
        var now = DateTime.UtcNow;
        return new MarketingWorkerStatusDto(
            true,
            true,
            await dbContext.Campaigns.CountAsync(x => x.TenantId == tenantId && x.Status == CampaignStatuses.Scheduled, cancellationToken),
            await dbContext.MarketingEmailDeliveries.CountAsync(x => x.TenantId == tenantId && (x.Status == "queued" || x.Status == "retrying") && x.NextAttemptAtUtc <= now, cancellationToken),
            await dbContext.JourneyStepExecutions.CountAsync(x => x.TenantId == tenantId && (x.Status == "queued" || x.Status == "retrying") && x.NextAttemptAtUtc <= now, cancellationToken),
            await dbContext.MarketingEmailDeliveries.CountAsync(x => x.TenantId == tenantId && x.Status == "failed", cancellationToken),
            now);
    }

    private async Task RecalculateRoiAsync(Guid tenantId, Guid campaignId, CancellationToken cancellationToken)
    {
        var projection = await dbContext.CampaignRoiProjections.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.CampaignId == campaignId, cancellationToken);
        if (projection is null)
        {
            projection = new CampaignRoiProjection(campaignId) { TenantId = tenantId };
            dbContext.CampaignRoiProjections.Add(projection);
        }

        var sent = await dbContext.MarketingEmailDeliveries.CountAsync(x => x.TenantId == tenantId && x.CampaignId == campaignId && x.Status == "sent", cancellationToken);
        var suppressed = await dbContext.MarketingEmailDeliveries.CountAsync(x => x.TenantId == tenantId && x.CampaignId == campaignId && x.Status == "suppressed", cancellationToken);
        var revenue = await dbContext.CampaignAttributions.Where(x => x.TenantId == tenantId && x.CampaignId == campaignId).SumAsync(x => x.RevenueAmount, cancellationToken);
        var cost = await dbContext.CampaignAttributions.Where(x => x.TenantId == tenantId && x.CampaignId == campaignId).SumAsync(x => x.CostAmount, cancellationToken);
        projection.Recalculate(sent, suppressed, revenue, cost);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static MarketingCampaignDto ToDto(Campaign x) => new(x.Id, x.Code, x.Name, x.Description, x.Status, x.SegmentId, x.EmailTemplateId, x.ScheduledAtUtc, x.BudgetAmount, x.ExpectedRevenueAmount, x.ApprovalStatus, x.UtmCampaign, x.IsActive);
    private static MarketingSegmentDto ToDto(Segment x) => new(x.Id, x.Code, x.Name, x.Description, x.SegmentType, x.CriteriaJson, x.IsSuppression, x.LastEvaluatedCount, x.LastEvaluatedAtUtc, x.IsActive);
    private static MarketingEmailTemplateDto ToDto(EmailCampaign x) => new(x.Id, x.Code, x.Name, x.Description, x.Subject, x.FromName, x.FromEmail, x.HtmlBody, x.TextBody, x.DeliverabilityStatus);
    private static MarketingJourneyDto ToDto(LeadNurturingJourney x) => new(x.Id, x.Code, x.Name, x.Description, x.Status, x.EntrySegmentId, x.StepDefinitionJson, x.StartedAtUtc, x.PausedAtUtc);
    private static MarketingSuppressionDto ToDto(SuppressionEntry x) => new(x.Id, x.EmailHash, x.EmailAddress, x.Reason, x.Source, x.SuppressedAtUtc);
    private static MarketingConsentDto ToDto(MarketingConsent x) => new(x.Id, x.EmailHash, x.EmailAddress, x.Status, x.Source, x.DoubleOptInRequired, x.GrantedAtUtc, x.RevokedAtUtc);
    private static MarketingRoiDto ToDto(CampaignRoiProjection x) => new(x.CampaignId, x.SentCount, x.SuppressedCount, x.RevenueAmount, x.CostAmount, x.RoiPercent, x.CalculatedAtUtc);
}
