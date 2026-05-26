// <copyright file="ArchiveKnowledgeBaseArticleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Abstractions.Persistence;
using NetMetric.Exceptions;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Articles.ArchiveKnowledgeBaseArticle;

public sealed class ArchiveKnowledgeBaseArticleCommandHandler(IKnowledgeBaseManagementDbContext dbContext) : IRequestHandler<ArchiveKnowledgeBaseArticleCommand>
{
    public async Task Handle(ArchiveKnowledgeBaseArticleCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Articles.FirstOrDefaultAsync(x => x.Id == request.ArticleId, cancellationToken)
            ?? throw new NotFoundAppException("Knowledge base article not found.");
        entity.Archive();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
