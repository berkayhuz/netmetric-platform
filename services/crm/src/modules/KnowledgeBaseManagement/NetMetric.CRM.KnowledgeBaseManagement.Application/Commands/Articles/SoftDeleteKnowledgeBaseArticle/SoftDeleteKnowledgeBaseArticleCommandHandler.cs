// <copyright file="SoftDeleteKnowledgeBaseArticleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Abstractions.Persistence;
using NetMetric.Exceptions;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.SoftDeleteKnowledgeBaseArticle;

public sealed class SoftDeleteKnowledgeBaseArticleCommandHandler(IKnowledgeBaseManagementDbContext dbContext) : IRequestHandler<SoftDeleteKnowledgeBaseArticleCommand>
{
    public async Task Handle(SoftDeleteKnowledgeBaseArticleCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Articles.FirstOrDefaultAsync(x => x.Id == request.ArticleId, cancellationToken)
            ?? throw new NotFoundAppException("Knowledge base article not found.");
        entity.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
