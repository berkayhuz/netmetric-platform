// <copyright file="CustomerIntelligenceController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.CustomerIntelligence.Application.Features.Cdp.Commands.ResolveIdentity;
using NetMetric.CRM.CustomerIntelligence.Application.Features.Cdp.Commands.TrackBehavioralEvent;
using NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360.Commands.AppendCustomerActivity;
using NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360.Commands.UpsertRelationship;
using NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360.Queries.GetCustomer360Workspace;
using NetMetric.CRM.CustomerIntelligence.Application.Features.Duplicates.Commands.DetectDuplicates;
using NetMetric.CRM.CustomerIntelligence.Application.Features.Insights.Queries.GetCustomerPortalSummary;
using NetMetric.CRM.CustomerIntelligence.Application.Features.Merges.Commands.MergeEntities;
using NetMetric.CRM.CustomerIntelligence.Application.Features.SavedViews.Commands.CreateSavedView;

namespace NetMetric.CRM.API.Controllers.Customers;

[ApiController]
[Route("api/customer-intelligence")]
[Authorize(Policy = AuthorizationPolicies.CustomerHealthRead)]
public sealed class CustomerIntelligenceController(IMediator mediator) : ControllerBase
{
    [HttpGet("customers/{customerId:guid}/workspace")]
    public async Task<IActionResult> GetWorkspace(Guid customerId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetCustomer360WorkspaceQuery(customerId), cancellationToken));

    [HttpGet("customers/{customerId:guid}/portal-summary")]
    public async Task<IActionResult> GetPortalSummary(Guid customerId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetCustomerPortalSummaryQuery(customerId), cancellationToken));

    [HttpPost("duplicates/detect")]
    [Authorize(Policy = AuthorizationPolicies.CustomerDuplicatesManage)]
    public async Task<IActionResult> DetectDuplicates([FromBody] DetectDuplicatesRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new DetectDuplicatesCommand(request.SubjectId, request.EntityType), cancellationToken);
        return Ok(new { id });
    }

    [HttpPost("merges")]
    [Authorize(Policy = AuthorizationPolicies.CustomerDuplicatesManage)]
    public async Task<IActionResult> MergeEntities([FromBody] MergeEntitiesRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new MergeEntitiesCommand(request.PrimaryEntityType, request.PrimaryEntityId, request.SecondaryEntityType, request.SecondaryEntityId, request.Reason), cancellationToken);
        return Ok(new { id });
    }

    [HttpPost("saved-views")]
    [Authorize(Policy = AuthorizationPolicies.CustomerSearchRead)]
    public async Task<IActionResult> CreateSavedView([FromBody] CreateSavedViewRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateSavedViewCommand(request.Name, request.Scope, request.FilterJson), cancellationToken);
        return CreatedAtAction(nameof(CreateSavedView), new { id }, new { id });
    }

    [HttpPost("activities")]
    [Authorize(Policy = AuthorizationPolicies.CustomerTimelineRead)]
    public async Task<IActionResult> AppendActivity([FromBody] AppendCustomerActivityRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new AppendCustomerActivityCommand(request.SubjectType, request.SubjectId, request.Name, request.Category, request.Channel, request.EntityType, request.RelatedEntityId, request.DataJson, request.OccurredAtUtc), cancellationToken));

    [HttpPut("relationships")]
    [Authorize(Policy = AuthorizationPolicies.CustomerTimelineRead)]
    public async Task<IActionResult> UpsertRelationship([FromBody] UpsertRelationshipRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new UpsertRelationshipCommand(request.SourceEntityType, request.SourceEntityId, request.TargetEntityType, request.TargetEntityId, request.Name, request.RelationshipType, request.StrengthScore, request.IsBidirectional, request.DataJson), cancellationToken));

    [HttpPost("cdp/events")]
    [Authorize(Policy = AuthorizationPolicies.CustomerTimelineRead)]
    public async Task<IActionResult> TrackEvent([FromBody] TrackBehavioralEventRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new TrackBehavioralEventCommand(request.Source, request.EventName, request.SubjectType, request.SubjectId, request.IdentityKey, request.Channel, request.PropertiesJson, request.OccurredAtUtc), cancellationToken));

    [HttpPost("cdp/identity-resolution")]
    [Authorize(Policy = AuthorizationPolicies.CustomerTimelineRead)]
    public async Task<IActionResult> ResolveIdentity([FromBody] ResolveIdentityRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ResolveIdentityCommand(request.SubjectType, request.SubjectId, request.IdentityType, request.IdentityValue, request.ConfidenceScore, request.ResolutionNotes), cancellationToken));

    public sealed record DetectDuplicatesRequest([property: JsonRequired] Guid SubjectId, string EntityType);

    public sealed record MergeEntitiesRequest(string PrimaryEntityType, [property: JsonRequired] Guid PrimaryEntityId, string SecondaryEntityType, [property: JsonRequired] Guid SecondaryEntityId, string Reason);

    public sealed record CreateSavedViewRequest(string Name, string Scope, string FilterJson);

    public sealed record AppendCustomerActivityRequest(string SubjectType, [property: JsonRequired] Guid SubjectId, string Name, string Category, string? Channel, string? EntityType, Guid? RelatedEntityId, string? DataJson, DateTime? OccurredAtUtc);

    public sealed record UpsertRelationshipRequest(string SourceEntityType, [property: JsonRequired] Guid SourceEntityId, string TargetEntityType, [property: JsonRequired] Guid TargetEntityId, string Name, string RelationshipType, [property: JsonRequired] decimal StrengthScore, [property: JsonRequired] bool IsBidirectional, string? DataJson);

    public sealed record TrackBehavioralEventRequest(string Source, string EventName, string SubjectType, [property: JsonRequired] Guid SubjectId, string? IdentityKey, string? Channel, string? PropertiesJson, DateTime? OccurredAtUtc);

    public sealed record ResolveIdentityRequest(string SubjectType, [property: JsonRequired] Guid SubjectId, string IdentityType, string IdentityValue, [property: JsonRequired] decimal ConfidenceScore, string? ResolutionNotes);
}
