// <copyright file="CreateKnowledgeBaseArticleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;
using NetMetric.CRM.KnowledgeBaseManagement.Domain.Entities;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.CreateKnowledgeBaseArticle;

public sealed class CreateKnowledgeBaseArticleCommandHandler(IKnowledgeBaseManagementDbContext dbContext) : IRequestHandler<CreateKnowledgeBaseArticleCommand, KnowledgeBaseArticleDetailDto>
{
    public async Task<KnowledgeBaseArticleDetailDto> Handle(CreateKnowledgeBaseArticleCommand request, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FirstAsync(x => x.Id == request.CategoryId, cancellationToken);
        var entity = new KnowledgeBaseArticle(request.CategoryId, request.Title, request.Summary, request.Content, request.IsPublic);
        await dbContext.Articles.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new KnowledgeBaseArticleDetailDto(entity.Id, entity.CategoryId, category.Name, entity.Title, entity.Slug, entity.Summary, entity.Content, entity.Status.ToString(), entity.IsPublic, entity.PublishedAt);
    }
}
