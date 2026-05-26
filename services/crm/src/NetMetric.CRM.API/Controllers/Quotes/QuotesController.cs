// <copyright file="QuotesController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.QuoteManagement.Application.Commands.ProposalTemplates;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Application.Queries.ProposalTemplates;
using NetMetric.CRM.QuoteManagement.Application.Queries.Quotes;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.API.Controllers.Quotes;

[ApiController]
[Route("api/quotes")]
[Authorize(Policy = AuthorizationPolicies.QuotesRead)]
public sealed class QuotesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(string? search, QuoteStatusType? status, Guid? opportunityId, Guid? customerId, Guid? ownerUserId, bool? isActive, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetQuotesQuery(search, status, opportunityId, customerId, ownerUserId, isActive, page, pageSize), cancellationToken));

    [HttpGet("{quoteId:guid}")]
    public async Task<IActionResult> Get(Guid quoteId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetQuoteByIdQuery(quoteId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{quoteId:guid}/workspace")]
    public async Task<IActionResult> GetWorkspace(Guid quoteId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetQuoteWorkspaceQuery(quoteId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{quoteId:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid quoteId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetQuoteTimelineQuery(quoteId), cancellationToken));

    [HttpGet("{quoteId:guid}/validation")]
    public async Task<IActionResult> Validate(Guid quoteId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new ValidateQuoteConfigurationQuery(quoteId), cancellationToken));

    [HttpGet("cpq/workspace")]
    public async Task<IActionResult> GetCpqWorkspace(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetCpqWorkspaceQuery(), cancellationToken));

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> Create([FromBody] QuoteUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateQuoteCommand(request.QuoteNumber, request.ProposalTitle, request.ProposalSummary, request.ProposalBody, request.QuoteDate, request.ValidUntil, request.OpportunityId, request.CustomerId, request.OwnerUserId, request.CurrencyCode, request.ExchangeRate, request.TermsAndConditions, request.ProposalTemplateId, request.Items), cancellationToken);
        return CreatedAtAction(nameof(Get), new { quoteId = result.Id }, result);
    }

    [HttpPut("{quoteId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> Update(Guid quoteId, [FromBody] QuoteUpdateRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new UpdateQuoteCommand(quoteId, request.QuoteNumber, request.ProposalTitle, request.ProposalSummary, request.ProposalBody, request.QuoteDate, request.ValidUntil, request.OpportunityId, request.CustomerId, request.OwnerUserId, request.CurrencyCode, request.ExchangeRate, request.TermsAndConditions, request.ProposalTemplateId, request.Items, request.RowVersion), cancellationToken));

    [HttpPost("{quoteId:guid}/submit")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> Submit(Guid quoteId, [FromBody] QuoteNoteRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new SubmitQuoteCommand(quoteId, request.Note, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpPost("{quoteId:guid}/approve")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> Approve(Guid quoteId, [FromBody] QuoteNoteRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ApproveQuoteCommand(quoteId, request.Note, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpPost("{quoteId:guid}/reject")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> Reject(Guid quoteId, [FromBody] QuoteReasonRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new RejectQuoteCommand(quoteId, request.Reason, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpPost("{quoteId:guid}/sent")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> MarkSent(Guid quoteId, [FromBody] QuoteDateNoteRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkQuoteSentCommand(quoteId, request.At, request.Note, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpPost("{quoteId:guid}/accepted")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> Accept(Guid quoteId, [FromBody] QuoteDateNoteRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new AcceptQuoteCommand(quoteId, request.At, request.Note, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpPost("{quoteId:guid}/declined")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> Decline(Guid quoteId, [FromBody] QuoteDeclineRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeclineQuoteCommand(quoteId, request.At, request.Reason, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpPost("{quoteId:guid}/expired")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> Expire(Guid quoteId, [FromBody] QuoteDateNoteRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ExpireQuoteCommand(quoteId, request.At, request.Note, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpPost("{quoteId:guid}/revisions")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> CreateRevision(Guid quoteId, [FromBody] CreateRevisionRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new CreateRevisionCommand(quoteId, request.NewQuoteNumber), cancellationToken));

    [HttpDelete("{quoteId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> Delete(Guid quoteId, CancellationToken cancellationToken)
    {
        await mediator.Send(new SoftDeleteQuoteCommand(quoteId), cancellationToken);
        return NoContent();
    }

    [HttpGet("proposal-templates")]
    [Authorize(Policy = AuthorizationPolicies.ProposalsRead)]
    public async Task<IActionResult> GetProposalTemplates(bool? isActive, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetProposalTemplatesQuery(isActive), cancellationToken));

    [HttpPost("proposal-templates")]
    [Authorize(Policy = AuthorizationPolicies.ProposalsManage)]
    public async Task<IActionResult> CreateProposalTemplate([FromBody] ProposalTemplateRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new CreateProposalTemplateCommand(request.Name, request.SubjectTemplate, request.BodyTemplate, request.IsDefault, request.IsActive, request.Notes), cancellationToken));

    [HttpPut("proposal-templates/{templateId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ProposalsManage)]
    public async Task<IActionResult> UpdateProposalTemplate(Guid templateId, [FromBody] ProposalTemplateRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new UpdateProposalTemplateCommand(templateId, request.Name, request.SubjectTemplate, request.BodyTemplate, request.IsDefault, request.IsActive, request.Notes), cancellationToken));

    [HttpDelete("proposal-templates/{templateId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ProposalsManage)]
    public async Task<IActionResult> DeleteProposalTemplate(Guid templateId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteProposalTemplateCommand(templateId), cancellationToken);
        return NoContent();
    }

    [HttpPost("cpq/guided-selling")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> RunGuidedSelling([FromBody] RunGuidedSellingRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new RunGuidedSellingQuery(request.Segment, request.Industry, request.Budget, request.RequiredCapabilities), cancellationToken));

    [HttpPut("cpq/guided-selling-playbooks/{playbookId:guid?}")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> UpsertGuidedSellingPlaybook(Guid? playbookId, [FromBody] UpsertGuidedSellingPlaybookRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new UpsertGuidedSellingPlaybookCommand(playbookId, request.Name, request.Segment, request.Industry, request.MinimumBudget, request.MaximumBudget, request.RequiredCapabilities, request.RecommendedBundleCodes, request.QualificationJson, request.RowVersion), cancellationToken));

    [HttpPut("cpq/product-bundles/{bundleId:guid?}")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> UpsertProductBundle(Guid? bundleId, [FromBody] UpsertProductBundleRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new UpsertProductBundleCommand(bundleId, request.Code, request.Name, request.Description, request.Segment, request.Industry, request.DiscountRate, request.MinimumBudget, request.Items, request.RowVersion), cancellationToken));

    [HttpPut("cpq/product-rules/{ruleId:guid?}")]
    [Authorize(Policy = AuthorizationPolicies.QuotesManage)]
    public async Task<IActionResult> UpsertProductRule(Guid? ruleId, [FromBody] UpsertProductRuleRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new UpsertProductRuleCommand(ruleId, request.Name, request.RuleType, request.TriggerProductId, request.TargetProductId, request.MinimumQuantity, request.MaximumDiscountRate, request.Severity, request.Message, request.CriteriaJson, request.RowVersion), cancellationToken));

    public sealed record QuoteUpsertRequest(string QuoteNumber, string? ProposalTitle, string? ProposalSummary, string? ProposalBody, [property: JsonRequired] DateTime QuoteDate, DateTime? ValidUntil, Guid? OpportunityId, Guid? CustomerId, Guid? OwnerUserId, string CurrencyCode, [property: JsonRequired] decimal ExchangeRate, string? TermsAndConditions, Guid? ProposalTemplateId, IReadOnlyList<QuoteLineInput> Items);

    public sealed record QuoteUpdateRequest(string QuoteNumber, string? ProposalTitle, string? ProposalSummary, string? ProposalBody, [property: JsonRequired] DateTime QuoteDate, DateTime? ValidUntil, Guid? OpportunityId, Guid? CustomerId, Guid? OwnerUserId, string CurrencyCode, [property: JsonRequired] decimal ExchangeRate, string? TermsAndConditions, Guid? ProposalTemplateId, IReadOnlyList<QuoteLineInput> Items, string RowVersion);

    public sealed record QuoteNoteRequest(string? Note, string? RowVersion);

    public sealed record QuoteReasonRequest(string Reason, string? RowVersion);

    public sealed record QuoteDateNoteRequest(DateTime? At, string? Note, string? RowVersion);

    public sealed record QuoteDeclineRequest(DateTime? At, string Reason, string? RowVersion);

    public sealed record CreateRevisionRequest(string NewQuoteNumber);

    public sealed record ProposalTemplateRequest(string Name, string? SubjectTemplate, string BodyTemplate, [property: JsonRequired] bool IsDefault, [property: JsonRequired] bool IsActive, string? Notes);

    public sealed record RunGuidedSellingRequest(string? Segment, string? Industry, decimal? Budget, IReadOnlyList<string> RequiredCapabilities);

    public sealed record UpsertGuidedSellingPlaybookRequest(string Name, string? Segment, string? Industry, decimal? MinimumBudget, decimal? MaximumBudget, string? RequiredCapabilities, IReadOnlyList<string> RecommendedBundleCodes, string? QualificationJson, string? RowVersion);

    public sealed record UpsertProductBundleRequest(string Code, string Name, string? Description, string? Segment, string? Industry, [property: JsonRequired] decimal DiscountRate, decimal? MinimumBudget, IReadOnlyList<ProductBundleLineInput> Items, string? RowVersion);

    public sealed record UpsertProductRuleRequest(string Name, string RuleType, Guid? TriggerProductId, Guid? TargetProductId, int? MinimumQuantity, decimal? MaximumDiscountRate, string Severity, string Message, string? CriteriaJson, string? RowVersion);
}
