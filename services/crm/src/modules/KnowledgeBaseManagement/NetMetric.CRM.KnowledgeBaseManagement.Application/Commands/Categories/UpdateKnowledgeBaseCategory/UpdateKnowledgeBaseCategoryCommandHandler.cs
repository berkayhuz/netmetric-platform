// <copyright file="UpdateKnowledgeBaseCategoryCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;
using NetMetric.Exceptions;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Categories.UpdateKnowledgeBaseCategory;

public sealed class UpdateKnowledgeBaseCategoryCommandHandler(IKnowledgeBaseManagementDbContext dbContext) : IRequestHandler<UpdateKnowledgeBaseCategoryCommand, KnowledgeBaseCategoryDto>
{
    public async Task<KnowledgeBaseCategoryDto> Handle(UpdateKnowledgeBaseCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == request.CategoryId, cancellationToken)
            ?? throw new NotFoundAppException("Knowledge base category not found.");

        entity.Update(request.Name, request.Description, request.SortOrder);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new KnowledgeBaseCategoryDto(entity.Id, entity.Name, entity.Slug, entity.Description, entity.SortOrder);
    }
}
