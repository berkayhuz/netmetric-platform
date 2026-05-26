// <copyright file="LeadsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.API.Security;
using NetMetric.CRM.LeadManagement.Application.Commands.Leads;
using NetMetric.CRM.LeadManagement.Application.Features.Bulk.Commands.BulkAssignLeadsOwner;
using NetMetric.CRM.LeadManagement.Application.Features.Bulk.Commands.BulkSoftDeleteLeads;
using NetMetric.CRM.LeadManagement.Application.Features.Conversions.Commands.ConvertLeadToCustomer;
using NetMetric.CRM.LeadManagement.Application.Features.Timeline.Queries.GetLeadTimeline;
using NetMetric.CRM.LeadManagement.Application.Features.Workspace.Queries.GetLeadWorkspace;
using NetMetric.CRM.LeadManagement.Application.Queries.Leads;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.API.Controllers.Leads;

[ApiController]
[Route("api/leads")]
[Authorize(Policy = AuthorizationPolicies.LeadsRead)]
public sealed class LeadsController(
    IMediator mediator,
    IConfiguration configuration,
    IPublicCaptureChallengeVerifier captureChallengeVerifier) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? search,
        [FromQuery] LeadStatusType? status,
        [FromQuery] LeadSourceType? source,
        [FromQuery] Guid? ownerUserId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetLeadsQuery(search, status, source, ownerUserId, isActive, page, pageSize), cancellationToken));

    [HttpGet("{leadId:guid}")]
    public async Task<IActionResult> Get(Guid leadId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLeadByIdQuery(leadId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{leadId:guid}/workspace")]
    public async Task<IActionResult> GetWorkspace(Guid leadId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLeadWorkspaceQuery(leadId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{leadId:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid leadId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetLeadTimelineQuery(leadId), cancellationToken));

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.LeadsManage)]
    public async Task<IActionResult> Create([FromBody] LeadUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateLeadCommand(
                request.FullName,
                request.CompanyName,
                request.Email,
                request.Phone,
                request.JobTitle,
                request.Description,
                request.EstimatedBudget,
                request.NextContactDate,
                request.Source,
                request.Status,
                request.Priority,
                request.CompanyId,
                request.OwnerUserId,
                request.Notes),
            cancellationToken);

        return CreatedAtAction(nameof(Get), new { leadId = result.Id }, result);
    }

    [HttpPut("{leadId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.LeadsManage)]
    public async Task<IActionResult> Update(Guid leadId, [FromBody] LeadUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateLeadCommand(
                leadId,
                request.FullName,
                request.CompanyName,
                request.Email,
                request.Phone,
                request.JobTitle,
                request.Description,
                request.EstimatedBudget,
                request.NextContactDate,
                request.Source,
                request.Status,
                request.Priority,
                request.CompanyId,
                request.OwnerUserId,
                request.Notes,
                request.RowVersion),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{leadId:guid}/owner")]
    [Authorize(Policy = AuthorizationPolicies.LeadsManage)]
    public async Task<IActionResult> AssignOwner(Guid leadId, [FromBody] AssignLeadOwnerRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new AssignLeadOwnerCommand(leadId, request.OwnerUserId), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{leadId:guid}/status")]
    [Authorize(Policy = AuthorizationPolicies.LeadsManage)]
    public async Task<IActionResult> ChangeStatus(Guid leadId, [FromBody] ChangeLeadStatusRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ChangeLeadStatusCommand(leadId, request.Status), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{leadId:guid}/next-contact")]
    [Authorize(Policy = AuthorizationPolicies.LeadsManage)]
    public async Task<IActionResult> ScheduleNextContact(Guid leadId, [FromBody] ScheduleNextContactRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ScheduleNextContactCommand(leadId, request.NextContactDate), cancellationToken);
        return NoContent();
    }

    [HttpPut("{leadId:guid}/score")]
    [Authorize(Policy = AuthorizationPolicies.LeadsManage)]
    public async Task<IActionResult> UpsertScore(Guid leadId, [FromBody] UpsertLeadScoreRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new UpsertLeadScoreCommand(leadId, request.Score, request.ScoreReason), cancellationToken));

    [HttpPut("{leadId:guid}/qualification")]
    [Authorize(Policy = AuthorizationPolicies.LeadsManage)]
    public async Task<IActionResult> UpsertQualification(Guid leadId, [FromBody] UpsertLeadQualificationRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpsertLeadQualificationCommand(leadId, request.FrameworkType, request.QualificationDataJson), cancellationToken);
        return NoContent();
    }

    [HttpPost("capture")]
    [AllowAnonymous] // Assuming capture endpoints might be called by webhooks/public forms
    [EnableRateLimiting("crm-public-capture")]
    [RequestSizeLimit(64_000)]
    public async Task<IActionResult> Capture([FromBody] CaptureLeadRequest request, CancellationToken cancellationToken)
    {
        if (!IsAllowedCaptureOrigin(Request, configuration) ||
            !string.IsNullOrWhiteSpace(request.Honeypot) ||
            (configuration.GetValue<bool>("Crm:PublicCapture:CaptchaRequired") &&
             string.IsNullOrWhiteSpace(request.CaptchaToken)) ||
            !await captureChallengeVerifier.VerifyAsync(request.CaptchaToken, HttpContext, cancellationToken))
        {
            return BadRequest(new { message = "Lead capture request could not be completed." });
        }

        var leadId = await mediator.Send(
            new CaptureLeadCommand(
                request.FullName,
                request.Email,
                request.Phone,
                request.CompanyName,
                request.JobTitle,
                request.Description,
                request.Source,
                request.CaptureFormId,
                request.ReferrerUrl,
                request.UtmSource,
                request.UtmMedium,
                request.UtmCampaign,
                request.UtmTerm,
                request.UtmContent,
                request.DynamicData),
            cancellationToken);

        return Ok(new { LeadId = leadId });
    }

    [HttpPost("{leadId:guid}/convert")]
    [Authorize(Policy = AuthorizationPolicies.LeadsManage)]
    public async Task<IActionResult> ConvertToCustomer(Guid leadId, [FromBody] ConvertLeadToCustomerRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ConvertLeadToCustomerCommand(leadId, request.CustomerType, request.MarkCustomerAsVip, request.CreateOpportunity, request.OpportunityName, request.EstimatedAmount, request.CompanyId), cancellationToken));

    [HttpPatch("owner")]
    [Authorize(Policy = AuthorizationPolicies.LeadsManage)]
    public async Task<IActionResult> BulkAssignOwner([FromBody] BulkAssignLeadsOwnerRequest request, CancellationToken cancellationToken) =>
        Ok(new { affected = await mediator.Send(new BulkAssignLeadsOwnerCommand(request.LeadIds, request.OwnerUserId), cancellationToken) });

    [HttpDelete("bulk")]
    [Authorize(Policy = AuthorizationPolicies.LeadsManage)]
    public async Task<IActionResult> BulkDelete([FromBody] BulkLeadIdsRequest request, CancellationToken cancellationToken) =>
        Ok(new { affected = await mediator.Send(new BulkSoftDeleteLeadsCommand(request.LeadIds), cancellationToken) });

    [HttpDelete("{leadId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.LeadsManage)]
    public async Task<IActionResult> Delete(Guid leadId, CancellationToken cancellationToken)
    {
        await mediator.Send(new SoftDeleteLeadCommand(leadId), cancellationToken);
        return NoContent();
    }

    public sealed record LeadUpsertRequest(
        string FullName,
        string? CompanyName,
        string? Email,
        string? Phone,
        string? JobTitle,
        string? Description,
        decimal? EstimatedBudget,
        DateTime? NextContactDate,
        [property: JsonRequired] LeadSourceType Source,
        [property: JsonRequired] LeadStatusType Status,
        [property: JsonRequired] PriorityType Priority,
        Guid? CompanyId,
        Guid? OwnerUserId,
        string? Notes);

    public sealed record LeadUpdateRequest(
        string FullName,
        string? CompanyName,
        string? Email,
        string? Phone,
        string? JobTitle,
        string? Description,
        decimal? EstimatedBudget,
        DateTime? NextContactDate,
        [property: JsonRequired] LeadSourceType Source,
        [property: JsonRequired] LeadStatusType Status,
        [property: JsonRequired] PriorityType Priority,
        Guid? CompanyId,
        Guid? OwnerUserId,
        string? Notes,
        string? RowVersion);

    public sealed record AssignLeadOwnerRequest(Guid? OwnerUserId);

    public sealed record ChangeLeadStatusRequest([property: JsonRequired] LeadStatusType Status);

    public sealed record ScheduleNextContactRequest(DateTime? NextContactDate);

    public sealed record UpsertLeadScoreRequest([property: JsonRequired] decimal Score, string? ScoreReason);

    public sealed record UpsertLeadQualificationRequest([property: JsonRequired] QualificationFrameworkType FrameworkType, string QualificationDataJson);

    public sealed record CaptureLeadRequest(
        string FullName,
        string? Email,
        string? Phone,
        string? CompanyName,
        string? JobTitle,
        string? Description,
        [property: JsonRequired] LeadSourceType Source,
        Guid? CaptureFormId,
        string? ReferrerUrl,
        string? UtmSource,
        string? UtmMedium,
        string? UtmCampaign,
        string? UtmTerm,
        string? UtmContent,
        Dictionary<string, object>? DynamicData,
        string? Honeypot,
        string? CaptchaToken);

    public sealed record ConvertLeadToCustomerRequest([property: JsonRequired] CustomerType CustomerType, [property: JsonRequired] bool MarkCustomerAsVip, [property: JsonRequired] bool CreateOpportunity, string? OpportunityName, decimal? EstimatedAmount, Guid? CompanyId);

    public sealed record BulkAssignLeadsOwnerRequest(IReadOnlyCollection<Guid> LeadIds, Guid? OwnerUserId);

    public sealed record BulkLeadIdsRequest(IReadOnlyCollection<Guid> LeadIds);

    private static bool IsAllowedCaptureOrigin(HttpRequest request, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Crm:PublicCapture:AllowedOrigins").Get<string[]>() ?? [];
        if (allowedOrigins.Length == 0)
        {
            return true;
        }

        var origin = request.Headers.Origin.FirstOrDefault();
        if (TryMatchesAllowedOrigin(origin, allowedOrigins))
        {
            return true;
        }

        var referer = request.Headers.Referer.FirstOrDefault();
        return TryMatchesAllowedOrigin(referer, allowedOrigins);
    }

    private static bool TryMatchesAllowedOrigin(string? value, IReadOnlyCollection<string> allowedOrigins)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !Uri.TryCreate(value, UriKind.Absolute, out var requestUri))
        {
            return false;
        }

        foreach (var allowed in allowedOrigins)
        {
            if (Uri.TryCreate(allowed, UriKind.Absolute, out var allowedUri) &&
                string.Equals(requestUri.Host, allowedUri.Host, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(requestUri.Scheme, allowedUri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
