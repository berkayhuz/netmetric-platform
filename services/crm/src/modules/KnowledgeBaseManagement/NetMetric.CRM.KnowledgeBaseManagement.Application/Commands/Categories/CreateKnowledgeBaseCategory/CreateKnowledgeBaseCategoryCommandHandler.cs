// <copyright file="CreateKnowledgeBaseCategoryCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.KnowledgeBaseManagement.Contracts.DTOs;
using NetMetric.CRM.KnowledgeBaseManagement.Domain.Entities;

namespace NetMetric.CRM.KnowledgeBaseManagement.Application.Commands.Categories.CreateKnowledgeBaseCategory;

public sealed class CreateKnowledgeBaseCategoryCommandHandler(IKnowledgeBaseManagementDbContext dbContext) : IRequestHandler<CreateKnowledgeBaseCategoryCommand, KnowledgeBaseCategoryDto>
{
    public async Task<KnowledgeBaseCategoryDto> Handle(CreateKnowledgeBaseCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = new KnowledgeBaseCategory(request.Name, request.Description, request.SortOrder);
        await dbContext.Categories.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new KnowledgeBaseCategoryDto(entity.Id, entity.Name, entity.Slug, entity.Description, entity.SortOrder);
    }
}
