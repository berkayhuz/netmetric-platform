// <copyright file="UpdateKnowledgeBaseArticleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;
using NetMetric.Exceptions;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.UpdateKnowledgeBaseArticle;

public sealed class UpdateKnowledgeBaseArticleCommandHandler(IKnowledgeBaseManagementDbContext dbContext) : IRequestHandler<UpdateKnowledgeBaseArticleCommand, KnowledgeBaseArticleDetailDto>
{
    public async Task<KnowledgeBaseArticleDetailDto> Handle(UpdateKnowledgeBaseArticleCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Articles.FirstOrDefaultAsync(x => x.Id == request.ArticleId, cancellationToken)
            ?? throw new NotFoundAppException("Knowledge base article not found.");
        var category = await dbContext.Categories.FirstAsync(x => x.Id == request.CategoryId, cancellationToken);
        entity.Update(request.CategoryId, request.Title, request.Summary, request.Content, request.IsPublic);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new KnowledgeBaseArticleDetailDto(entity.Id, entity.CategoryId, category.Name, entity.Title, entity.Slug, entity.Summary, entity.Content, entity.Status.ToString(), entity.IsPublic, entity.PublishedAt);
    }
}
