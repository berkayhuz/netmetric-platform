// <copyright file="GetKnowledgeBaseArticlesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Queries.Articles.GetKnowledgeBaseArticles;

public sealed class GetKnowledgeBaseArticlesQueryHandler(IKnowledgeBaseManagementDbContext dbContext) : IRequestHandler<GetKnowledgeBaseArticlesQuery, PagedResult<KnowledgeBaseArticleListItemDto>>
{
    public async Task<PagedResult<KnowledgeBaseArticleListItemDto>> Handle(GetKnowledgeBaseArticlesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 20 : request.PageSize, 1, 200);

        var query = from article in dbContext.Articles
                    join category in dbContext.Categories on article.CategoryId equals category.Id
                    select new { article, category };

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x => x.article.Title.Contains(request.Search) || (x.article.Summary != null && x.article.Summary.Contains(request.Search)));

        if (request.CategoryId.HasValue)
            query = query.Where(x => x.article.CategoryId == request.CategoryId.Value);

        if (request.PublishedOnly == true)
            query = query.Where(x => x.article.Status == Domain.Enums.KnowledgeBaseArticleStatus.Published);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.article.PublishedAt).ThenBy(x => x.article.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new KnowledgeBaseArticleListItemDto(x.article.Id, x.article.CategoryId, x.category.Name, x.article.Title, x.article.Slug, x.article.Summary, x.article.Status.ToString(), x.article.IsPublic, x.article.PublishedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<KnowledgeBaseArticleListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }
}
