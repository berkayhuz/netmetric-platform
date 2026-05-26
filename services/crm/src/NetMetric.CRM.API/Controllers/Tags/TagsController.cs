// <copyright file="TagsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.TagManagement.Application.Features.Classifications.Commands.CreateClassificationScheme;
using NetMetric.CRM.TagManagement.Application.Features.Classifications.Queries.GetClassificationSchemes;
using NetMetric.CRM.TagManagement.Application.Features.SmartLabels.Commands.CreateSmartLabelRule;
using NetMetric.CRM.TagManagement.Application.Features.SmartLabels.Queries.GetSmartLabelRules;
using NetMetric.CRM.TagManagement.Application.Features.TagGroups.Commands.CreateTagGroup;
using NetMetric.CRM.TagManagement.Application.Features.TagGroups.Queries.GetTagGroups;
using NetMetric.CRM.TagManagement.Application.Features.Tags.Commands.CreateTag;
using NetMetric.CRM.TagManagement.Application.Features.Tags.Queries.GetTags;

namespace NetMetric.CRM.API.Controllers.Tags;

[ApiController]
[Route("api/tags")]
[Authorize(Policy = AuthorizationPolicies.TagsRead)]
public sealed class TagsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListTags(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTagsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("groups")]
    public async Task<IActionResult> ListGroups(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTagGroupsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("smart-label-rules")]
    public async Task<IActionResult> ListSmartLabelRules(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSmartLabelRulesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("classification-schemes")]
    public async Task<IActionResult> ListClassificationSchemes(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetClassificationSchemesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.TagsManage)]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateTagCommand(request.Name, request.Color), cancellationToken);
        return CreatedAtAction(nameof(CreateTag), new { id }, new { id });
    }

    [HttpPost("groups")]
    [Authorize(Policy = AuthorizationPolicies.TagsManage)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateTagGroupRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateTagGroupCommand(request.Name, request.Color), cancellationToken);
        return CreatedAtAction(nameof(CreateGroup), new { id }, new { id });
    }

    [HttpPost("smart-label-rules")]
    [Authorize(Policy = AuthorizationPolicies.SmartLabelsManage)]
    public async Task<IActionResult> CreateSmartLabelRule([FromBody] CreateSmartLabelRuleRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateSmartLabelRuleCommand(request.Name, request.EntityType, request.ConditionJson), cancellationToken);
        return CreatedAtAction(nameof(CreateSmartLabelRule), new { id }, new { id });
    }

    [HttpPost("classification-schemes")]
    [Authorize(Policy = AuthorizationPolicies.ClassificationsManage)]
    public async Task<IActionResult> CreateClassificationScheme([FromBody] CreateClassificationSchemeRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateClassificationSchemeCommand(request.Name, request.EntityType), cancellationToken);
        return CreatedAtAction(nameof(CreateClassificationScheme), new { id }, new { id });
    }

    public sealed record CreateTagRequest(string Name, string Color);

    public sealed record CreateTagGroupRequest(string Name, string? Color);

    public sealed record CreateSmartLabelRuleRequest(string Name, string EntityType, string ConditionJson);

    public sealed record CreateClassificationSchemeRequest(string Name, string EntityType);
}
