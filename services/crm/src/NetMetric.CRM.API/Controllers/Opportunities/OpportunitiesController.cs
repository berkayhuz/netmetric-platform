// <copyright file="OpportunitiesController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.OpportunityManagement.Application.Commands;
using NetMetric.CRM.OpportunityManagement.Application.Features.Bulk.Commands.BulkAssignOpportunitiesOwner;
using NetMetric.CRM.OpportunityManagement.Application.Features.Bulk.Commands.BulkChangeOpportunityStage;
using NetMetric.CRM.OpportunityManagement.Application.Features.LostReasons.Queries.GetLostReasons;
using NetMetric.CRM.OpportunityManagement.Application.Features.Pipeline.Queries.GetPipelineBoard;
using NetMetric.CRM.OpportunityManagement.Application.Features.Quotes.Commands.CreateQuote;
using NetMetric.CRM.OpportunityManagement.Application.Features.Quotes.Queries.GetQuotesByOpportunity;
using NetMetric.CRM.OpportunityManagement.Application.Features.Timeline.Queries.GetOpportunityTimeline;
using NetMetric.CRM.OpportunityManagement.Application.Features.Workspace.Queries.GetOpportunityWorkspace;
using NetMetric.CRM.OpportunityManagement.Application.Queries.Opportunities;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.API.Controllers.Opportunities;

[ApiController]
[Route("api/opportunities")]
[Authorize(Policy = AuthorizationPolicies.OpportunitiesRead)]
public sealed class OpportunitiesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(string? search, OpportunityStageType? stage, OpportunityStatusType? status, Guid? ownerUserId, Guid? leadId, Guid? customerId, bool? isActive, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetOpportunitiesQuery(search, stage, status, ownerUserId, leadId, customerId, isActive, page, pageSize), cancellationToken));

    [HttpGet("{opportunityId:guid}")]
    public async Task<IActionResult> Get(Guid opportunityId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOpportunityByIdQuery(opportunityId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{opportunityId:guid}/workspace")]
    public async Task<IActionResult> GetWorkspace(Guid opportunityId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOpportunityWorkspaceQuery(opportunityId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{opportunityId:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid opportunityId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetOpportunityTimelineQuery(opportunityId), cancellationToken));

    [HttpGet("pipeline-board")]
    public async Task<IActionResult> GetPipelineBoard(Guid? ownerUserId, string? search, int maxItemsPerStage = 25, CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetPipelineBoardQuery(ownerUserId, search, maxItemsPerStage), cancellationToken));

    [HttpGet("lost-reasons")]
    public async Task<IActionResult> GetLostReasons(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetLostReasonsQuery(), cancellationToken));

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.OpportunitiesManage)]
    public async Task<IActionResult> Create([FromBody] OpportunityUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateOpportunityCommand(request.OpportunityCode, request.Name, request.Description, request.EstimatedAmount, request.ExpectedRevenue, request.Probability, request.EstimatedCloseDate, request.Stage, request.Status, request.Priority, request.LeadId, request.CustomerId, request.OwnerUserId, request.Notes), cancellationToken);
        return CreatedAtAction(nameof(Get), new { opportunityId = result.Id }, result);
    }

    [HttpPut("{opportunityId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.OpportunitiesManage)]
    public async Task<IActionResult> Update(Guid opportunityId, [FromBody] OpportunityUpdateRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new UpdateOpportunityCommand(opportunityId, request.OpportunityCode, request.Name, request.Description, request.EstimatedAmount, request.ExpectedRevenue, request.Probability, request.EstimatedCloseDate, request.Stage, request.Status, request.Priority, request.LeadId, request.CustomerId, request.OwnerUserId, request.Notes, request.RowVersion), cancellationToken));

    [HttpPatch("{opportunityId:guid}/owner")]
    [Authorize(Policy = AuthorizationPolicies.OpportunitiesManage)]
    public async Task<IActionResult> AssignOwner(Guid opportunityId, [FromBody] AssignOpportunityOwnerRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new AssignOpportunityOwnerCommand(opportunityId, request.OwnerUserId), cancellationToken);
        return NoContent();
    }

    [HttpPatch("{opportunityId:guid}/stage")]
    [Authorize(Policy = AuthorizationPolicies.OpportunitiesManage)]
    public async Task<IActionResult> ChangeStage(Guid opportunityId, [FromBody] ChangeOpportunityStageRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ChangeOpportunityStageCommand(opportunityId, request.NewStage, request.Note, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpPost("{opportunityId:guid}/won")]
    [Authorize(Policy = AuthorizationPolicies.OpportunitiesManage)]
    public async Task<IActionResult> MarkWon(Guid opportunityId, [FromBody] MarkOpportunityWonRequest request, CancellationToken cancellationToken) =>
        Ok(new { dealId = await mediator.Send(new MarkOpportunityWonCommand(opportunityId, request.DealName, request.ClosedDate, request.RowVersion), cancellationToken) });

    [HttpPost("{opportunityId:guid}/lost")]
    [Authorize(Policy = AuthorizationPolicies.OpportunitiesManage)]
    public async Task<IActionResult> MarkLost(Guid opportunityId, [FromBody] MarkOpportunityLostRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkOpportunityLostCommand(opportunityId, request.LostReasonId, request.LostNote, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpPost("{opportunityId:guid}/contacts")]
    [Authorize(Policy = AuthorizationPolicies.OpportunitiesManage)]
    public async Task<IActionResult> AddContact(Guid opportunityId, [FromBody] AddOpportunityContactRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new AddOpportunityContactCommand(opportunityId, request.ContactId, request.IsDecisionMaker, request.IsPrimary), cancellationToken));

    [HttpPost("{opportunityId:guid}/products")]
    [Authorize(Policy = AuthorizationPolicies.OpportunitiesManage)]
    public async Task<IActionResult> AddProduct(Guid opportunityId, [FromBody] AddOpportunityProductRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new AddOpportunityProductCommand(opportunityId, request.ProductId, request.Quantity, request.UnitPrice, request.DiscountRate, request.VatRate), cancellationToken));

    [HttpGet("{opportunityId:guid}/quotes")]
    [Authorize(Policy = AuthorizationPolicies.OpportunityQuotesRead)]
    public async Task<IActionResult> GetQuotes(Guid opportunityId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetQuotesByOpportunityQuery(opportunityId), cancellationToken));

    [HttpPost("{opportunityId:guid}/quotes")]
    [Authorize(Policy = AuthorizationPolicies.OpportunityQuotesManage)]
    public async Task<IActionResult> CreateQuote(Guid opportunityId, [FromBody] CreateOpportunityQuoteRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new CreateQuoteCommand(opportunityId, request.QuoteNumber, request.QuoteDate, request.ValidUntil, request.TermsAndConditions, request.OwnerUserId, request.CurrencyCode, request.ExchangeRate, request.Items), cancellationToken));

    [HttpPatch("owner")]
    [Authorize(Policy = AuthorizationPolicies.OpportunitiesManage)]
    public async Task<IActionResult> BulkAssignOwner([FromBody] BulkAssignOpportunitiesOwnerRequest request, CancellationToken cancellationToken) =>
        Ok(new { affected = await mediator.Send(new BulkAssignOpportunitiesOwnerCommand(request.OpportunityIds, request.OwnerUserId), cancellationToken) });

    [HttpPatch("stage")]
    [Authorize(Policy = AuthorizationPolicies.OpportunitiesManage)]
    public async Task<IActionResult> BulkChangeStage([FromBody] BulkChangeOpportunityStageRequest request, CancellationToken cancellationToken) =>
        Ok(new { affected = await mediator.Send(new BulkChangeOpportunityStageCommand(request.OpportunityIds, request.NewStage, request.Note), cancellationToken) });

    [HttpDelete("{opportunityId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.OpportunitiesManage)]
    public async Task<IActionResult> Delete(Guid opportunityId, CancellationToken cancellationToken)
    {
        await mediator.Send(new SoftDeleteOpportunityCommand(opportunityId), cancellationToken);
        return NoContent();
    }

    public sealed record OpportunityUpsertRequest(string OpportunityCode, string Name, string? Description, [property: JsonRequired] decimal EstimatedAmount, decimal? ExpectedRevenue, [property: JsonRequired] decimal Probability, DateTime? EstimatedCloseDate, [property: JsonRequired] OpportunityStageType Stage, [property: JsonRequired] OpportunityStatusType Status, [property: JsonRequired] PriorityType Priority, Guid? LeadId, Guid? CustomerId, Guid? OwnerUserId, string? Notes);

    public sealed record OpportunityUpdateRequest(string OpportunityCode, string Name, string? Description, [property: JsonRequired] decimal EstimatedAmount, decimal? ExpectedRevenue, [property: JsonRequired] decimal Probability, DateTime? EstimatedCloseDate, [property: JsonRequired] OpportunityStageType Stage, [property: JsonRequired] OpportunityStatusType Status, [property: JsonRequired] PriorityType Priority, Guid? LeadId, Guid? CustomerId, Guid? OwnerUserId, string? Notes, string RowVersion);

    public sealed record AssignOpportunityOwnerRequest(Guid? OwnerUserId);

    public sealed record ChangeOpportunityStageRequest([property: JsonRequired] OpportunityStageType NewStage, string? Note, string? RowVersion);

    public sealed record MarkOpportunityWonRequest(string? DealName, [property: JsonRequired] DateTime ClosedDate, string? RowVersion);

    public sealed record MarkOpportunityLostRequest(Guid? LostReasonId, string? LostNote, string? RowVersion);

    public sealed record AddOpportunityContactRequest([property: JsonRequired] Guid ContactId, [property: JsonRequired] bool IsDecisionMaker, [property: JsonRequired] bool IsPrimary);

    public sealed record AddOpportunityProductRequest([property: JsonRequired] Guid ProductId, [property: JsonRequired] int Quantity, [property: JsonRequired] decimal UnitPrice, [property: JsonRequired] decimal DiscountRate, [property: JsonRequired] decimal VatRate);

    public sealed record CreateOpportunityQuoteRequest(string QuoteNumber, [property: JsonRequired] DateTime QuoteDate, DateTime? ValidUntil, string? TermsAndConditions, Guid? OwnerUserId, string CurrencyCode, [property: JsonRequired] decimal ExchangeRate, IReadOnlyList<CreateQuoteItemModel> Items);

    public sealed record BulkAssignOpportunitiesOwnerRequest(IReadOnlyCollection<Guid> OpportunityIds, Guid? OwnerUserId);

    public sealed record BulkChangeOpportunityStageRequest(IReadOnlyCollection<Guid> OpportunityIds, [property: JsonRequired] OpportunityStageType NewStage, string? Note);
}
