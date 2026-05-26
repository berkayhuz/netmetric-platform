// <copyright file="GetKnowledgeBaseArticleBySlugQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Queries.Articles.GetKnowledgeBaseArticleBySlug;

public sealed class GetKnowledgeBaseArticleBySlugQueryHandler(IKnowledgeBaseManagementDbContext dbContext) : IRequestHandler<GetKnowledgeBaseArticleBySlugQuery, KnowledgeBaseArticleDetailDto?>
{
    public async Task<KnowledgeBaseArticleDetailDto?> Handle(GetKnowledgeBaseArticleBySlugQuery request, CancellationToken cancellationToken)
        => await (from article in dbContext.Articles
                  join category in dbContext.Categories on article.CategoryId equals category.Id
                  where article.Slug == request.Slug
                  select new KnowledgeBaseArticleDetailDto(article.Id, article.CategoryId, category.Name, article.Title, article.Slug, article.Summary, article.Content, article.Status.ToString(), article.IsPublic, article.PublishedAt))
            .FirstOrDefaultAsync(cancellationToken);
}
