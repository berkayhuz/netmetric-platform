// <copyright file="CampaignsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Consents;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Security;

namespace NetMetric.CRM.API.Controllers.Marketings;

[ApiController]
[Route("api/marketing/tenants/{tenantId:guid}")]
[Authorize(Policy = AuthorizationPolicies.CampaignsRead)]
public sealed class CampaignsController(
    IMarketingAutomationService marketing,
    IMarketingConsentTokenService consentTokens) : ControllerBase
{
    [HttpGet("campaigns")]
    public Task<IActionResult> ListCampaigns(Guid tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null, CancellationToken cancellationToken = default)
        => OkAsync(marketing.ListCampaignsAsync(tenantId, page, pageSize, status, cancellationToken));

    [HttpGet("campaigns/{campaignId:guid}")]
    public async Task<IActionResult> GetCampaign(Guid tenantId, Guid campaignId, CancellationToken cancellationToken)
        => await marketing.GetCampaignAsync(tenantId, campaignId, cancellationToken) is { } result ? Ok(result) : NotFound();

    [HttpPost("campaigns")]
    [Authorize(Policy = AuthorizationPolicies.CampaignsManage)]
    public async Task<IActionResult> CreateCampaign(Guid tenantId, [FromBody] MarketingCampaignUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await marketing.CreateCampaignAsync(tenantId, request, cancellationToken);
        return CreatedAtAction(nameof(GetCampaign), new { tenantId, campaignId = result.Id }, result);
    }

    [HttpPut("campaigns/{campaignId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CampaignsManage)]
    public Task<IActionResult> UpdateCampaign(Guid tenantId, Guid campaignId, [FromBody] MarketingCampaignUpsertRequest request, CancellationToken cancellationToken)
        => OkAsync(marketing.UpdateCampaignAsync(tenantId, campaignId, request, cancellationToken));

    [HttpPost("campaigns/{campaignId:guid}/schedule")]
    [Authorize(Policy = AuthorizationPolicies.CampaignsManage)]
    public async Task<IActionResult> ScheduleCampaign(Guid tenantId, Guid campaignId, [FromBody] MarketingScheduleRequest request, CancellationToken cancellationToken)
    {
        await marketing.ScheduleCampaignAsync(tenantId, campaignId, request.ScheduledAtUtc, cancellationToken);
        return NoContent();
    }

    [HttpPost("campaigns/{campaignId:guid}/pause")]
    [Authorize(Policy = AuthorizationPolicies.CampaignsManage)]
    public async Task<IActionResult> PauseCampaign(Guid tenantId, Guid campaignId, CancellationToken cancellationToken)
    {
        await marketing.PauseCampaignAsync(tenantId, campaignId, cancellationToken);
        return NoContent();
    }

    [HttpPost("campaigns/{campaignId:guid}/resume")]
    [Authorize(Policy = AuthorizationPolicies.CampaignsManage)]
    public async Task<IActionResult> ResumeCampaign(Guid tenantId, Guid campaignId, [FromBody] MarketingScheduleRequest request, CancellationToken cancellationToken)
    {
        await marketing.ResumeCampaignAsync(tenantId, campaignId, request.ScheduledAtUtc, cancellationToken);
        return NoContent();
    }

    [HttpPost("campaigns/{campaignId:guid}/cancel")]
    [Authorize(Policy = AuthorizationPolicies.CampaignsManage)]
    public async Task<IActionResult> CancelCampaign(Guid tenantId, Guid campaignId, CancellationToken cancellationToken)
    {
        await marketing.CancelCampaignAsync(tenantId, campaignId, cancellationToken);
        return NoContent();
    }

    [HttpGet("segments")]
    public Task<IActionResult> ListSegments(Guid tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => OkAsync(marketing.ListSegmentsAsync(tenantId, page, pageSize, cancellationToken));

    [HttpGet("segments/{segmentId:guid}")]
    public async Task<IActionResult> GetSegment(Guid tenantId, Guid segmentId, CancellationToken cancellationToken)
        => await marketing.GetSegmentAsync(tenantId, segmentId, cancellationToken) is { } result ? Ok(result) : NotFound();

    [HttpPost("segments/{segmentId:guid}/evaluate")]
    public Task<IActionResult> EvaluateSegment(Guid tenantId, Guid segmentId, [FromBody] MarketingAudienceRequest request, CancellationToken cancellationToken)
        => OkAsync(marketing.EvaluateSegmentAsync(tenantId, segmentId, request.Audience, cancellationToken));

    [HttpGet("suppressions")]
    public Task<IActionResult> ListSuppressions(Guid tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => OkAsync(marketing.ListSuppressionsAsync(tenantId, page, pageSize, cancellationToken));

    [HttpPost("suppressions")]
    [Authorize(Policy = AuthorizationPolicies.CampaignsManage)]
    public Task<IActionResult> AddSuppression(Guid tenantId, [FromBody] MarketingSuppressionRequest request, CancellationToken cancellationToken)
        => OkAsync(marketing.AddSuppressionAsync(tenantId, request, cancellationToken));

    [HttpGet("email-templates")]
    public Task<IActionResult> ListTemplates(Guid tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => OkAsync(marketing.ListTemplatesAsync(tenantId, page, pageSize, cancellationToken));

    [HttpGet("email-templates/{templateId:guid}")]
    public async Task<IActionResult> GetTemplate(Guid tenantId, Guid templateId, CancellationToken cancellationToken)
        => await marketing.GetTemplateAsync(tenantId, templateId, cancellationToken) is { } result ? Ok(result) : NotFound();

    [HttpPost("email-templates/{templateId:guid}/preview")]
    public Task<IActionResult> PreviewTemplate(Guid tenantId, Guid templateId, [FromBody] MarketingTemplatePreviewRequest request, CancellationToken cancellationToken)
        => OkAsync(marketing.PreviewTemplateAsync(tenantId, templateId, request.PayloadJson, cancellationToken));

    [HttpGet("journeys")]
    public Task<IActionResult> ListJourneys(Guid tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => OkAsync(marketing.ListJourneysAsync(tenantId, page, pageSize, cancellationToken));

    [HttpGet("journeys/{journeyId:guid}")]
    public async Task<IActionResult> GetJourney(Guid tenantId, Guid journeyId, CancellationToken cancellationToken)
        => await marketing.GetJourneyAsync(tenantId, journeyId, cancellationToken) is { } result ? Ok(result) : NotFound();

    [HttpPost("journeys/{journeyId:guid}/start")]
    [Authorize(Policy = AuthorizationPolicies.CampaignsManage)]
    public async Task<IActionResult> StartJourney(Guid tenantId, Guid journeyId, [FromBody] MarketingAudienceRequest request, CancellationToken cancellationToken)
    {
        await marketing.StartJourneyAsync(tenantId, journeyId, request.Audience, cancellationToken);
        return NoContent();
    }

    [HttpPost("journeys/{journeyId:guid}/pause")]
    [Authorize(Policy = AuthorizationPolicies.CampaignsManage)]
    public async Task<IActionResult> PauseJourney(Guid tenantId, Guid journeyId, CancellationToken cancellationToken)
    {
        await marketing.PauseJourneyAsync(tenantId, journeyId, cancellationToken);
        return NoContent();
    }

    [HttpGet("campaigns/{campaignId:guid}/roi")]
    public Task<IActionResult> GetRoi(Guid tenantId, Guid campaignId, CancellationToken cancellationToken)
        => OkAsync(marketing.GetRoiAsync(tenantId, campaignId, cancellationToken));

    [HttpPost("consent")]
    [AllowAnonymous]
    [EnableRateLimiting("crm-marketing-consent")]
    [RequestSizeLimit(16_384)]
    public async Task<IActionResult> UpsertConsent(Guid tenantId, [FromBody] SignedMarketingConsentRequest request, CancellationToken cancellationToken)
    {
        var token = await consentTokens.ValidateAndConsumeAsync(
            tenantId,
            request.Token,
            MarketingConsentTokenPurposes.Consent,
            cancellationToken);
        if (!token.IsValid || string.IsNullOrWhiteSpace(token.EmailAddress) || string.IsNullOrWhiteSpace(token.Source))
        {
            return BadRequest(new { message = "Consent request could not be completed." });
        }

        return Ok(await marketing.UpsertConsentAsync(
            tenantId,
            new MarketingConsentRequest(
                token.EmailAddress,
                string.IsNullOrWhiteSpace(token.Status) ? MarketingConsentStatuses.Granted : token.Status,
                token.Source,
                token.DoubleOptInRequired),
            cancellationToken));
    }

    [HttpPost("unsubscribe")]
    [AllowAnonymous]
    [EnableRateLimiting("crm-marketing-consent")]
    [RequestSizeLimit(16_384)]
    public async Task<IActionResult> Unsubscribe(Guid tenantId, [FromBody] SignedMarketingUnsubscribeRequest request, CancellationToken cancellationToken)
    {
        var token = await consentTokens.ValidateAndConsumeAsync(
            tenantId,
            request.Token,
            MarketingConsentTokenPurposes.Unsubscribe,
            cancellationToken);
        if (!token.IsValid || string.IsNullOrWhiteSpace(token.EmailAddress) || string.IsNullOrWhiteSpace(token.Source))
        {
            return BadRequest(new { message = "Unsubscribe request could not be completed." });
        }

        return Ok(await marketing.UnsubscribeAsync(
            tenantId,
            new MarketingUnsubscribeRequest(token.EmailAddress, token.Source),
            cancellationToken));
    }

    [HttpGet("worker-status")]
    public Task<IActionResult> GetWorkerStatus(Guid tenantId, CancellationToken cancellationToken)
        => OkAsync(marketing.GetWorkerStatusAsync(tenantId, cancellationToken));

    private static async Task<IActionResult> OkAsync<T>(Task<T> task) => new OkObjectResult(await task);
}

public sealed record MarketingScheduleRequest([property: JsonRequired] DateTime ScheduledAtUtc);
public sealed record MarketingAudienceRequest(IReadOnlyCollection<MarketingAudienceMemberInput> Audience);
public sealed record MarketingTemplatePreviewRequest(string PayloadJson);
public sealed record SignedMarketingConsentRequest([property: JsonRequired] string Token);
public sealed record SignedMarketingUnsubscribeRequest([property: JsonRequired] string Token);
