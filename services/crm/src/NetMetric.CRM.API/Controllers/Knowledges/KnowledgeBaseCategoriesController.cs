// <copyright file="KnowledgeBaseCategoriesController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Categories.CreateKnowledgeBaseCategory;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Categories.UpdateKnowledgeBaseCategory;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Queries.Categories.GetKnowledgeBaseCategories;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.Requests;

namespace NetMetric.CRM.API.Controllers.Knowledges;

[ApiController]
[Route("api/knowledge-base/categories")]
[Authorize(Policy = AuthorizationPolicies.KnowledgeBaseCategoriesRead)]
public sealed class KnowledgeBaseCategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<KnowledgeBaseCategoryDto>>> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetKnowledgeBaseCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.KnowledgeBaseCategoriesManage)]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> Create(
        [FromBody] KnowledgeBaseCategoryUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateKnowledgeBaseCategoryCommand(request.Name, request.Description, request.SortOrder),
            cancellationToken);

        return CreatedAtAction(nameof(Get), new { categoryId = result.Id }, result);
    }

    [HttpPut("{categoryId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.KnowledgeBaseCategoriesManage)]
    public async Task<ActionResult<KnowledgeBaseCategoryDto>> Update(
        Guid categoryId,
        [FromBody] KnowledgeBaseCategoryUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateKnowledgeBaseCategoryCommand(categoryId, request.Name, request.Description, request.SortOrder),
            cancellationToken);

        return Ok(result);
    }
}
