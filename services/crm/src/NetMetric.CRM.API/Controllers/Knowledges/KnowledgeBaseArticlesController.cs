// <copyright file="KnowledgeBaseArticlesController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.ArchiveKnowledgeBaseArticle;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.CreateKnowledgeBaseArticle;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.PublishKnowledgeBaseArticle;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.SoftDeleteKnowledgeBaseArticle;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.UpdateKnowledgeBaseArticle;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Queries.Articles.GetKnowledgeBaseArticleBySlug;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Queries.Articles.GetKnowledgeBaseArticles;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.Requests;
using NetMetric.Pagination;

namespace NetMetric.CRM.API.Controllers.Knowledges;

[ApiController]
[Route("api/knowledge-base/articles")]
[Authorize(Policy = AuthorizationPolicies.KnowledgeBaseArticlesRead)]
public sealed class KnowledgeBaseArticlesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<KnowledgeBaseArticleListItemDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? publishedOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetKnowledgeBaseArticlesQuery(search, categoryId, publishedOnly, page, pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<KnowledgeBaseArticleDetailDto>> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetKnowledgeBaseArticleBySlugQuery(slug), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.KnowledgeBaseArticlesManage)]
    public async Task<ActionResult<KnowledgeBaseArticleDetailDto>> Create(
        [FromBody] KnowledgeBaseArticleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateKnowledgeBaseArticleCommand(request.CategoryId, request.Title, request.Summary, request.Content, request.IsPublic),
            cancellationToken);

        return CreatedAtAction(nameof(GetBySlug), new { slug = result.Slug }, result);
    }

    [HttpPut("{articleId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.KnowledgeBaseArticlesManage)]
    public async Task<ActionResult<KnowledgeBaseArticleDetailDto>> Update(
        Guid articleId,
        [FromBody] KnowledgeBaseArticleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateKnowledgeBaseArticleCommand(articleId, request.CategoryId, request.Title, request.Summary, request.Content, request.IsPublic),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{articleId:guid}/publish")]
    [Authorize(Policy = AuthorizationPolicies.KnowledgeBaseArticlesManage)]
    public async Task<IActionResult> Publish(Guid articleId, CancellationToken cancellationToken)
    {
        await mediator.Send(new PublishKnowledgeBaseArticleCommand(articleId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{articleId:guid}/archive")]
    [Authorize(Policy = AuthorizationPolicies.KnowledgeBaseArticlesManage)]
    public async Task<IActionResult> Archive(Guid articleId, CancellationToken cancellationToken)
    {
        await mediator.Send(new ArchiveKnowledgeBaseArticleCommand(articleId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{articleId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.KnowledgeBaseArticlesManage)]
    public async Task<IActionResult> Delete(Guid articleId, CancellationToken cancellationToken)
    {
        await mediator.Send(new SoftDeleteKnowledgeBaseArticleCommand(articleId), cancellationToken);
        return NoContent();
    }
}
