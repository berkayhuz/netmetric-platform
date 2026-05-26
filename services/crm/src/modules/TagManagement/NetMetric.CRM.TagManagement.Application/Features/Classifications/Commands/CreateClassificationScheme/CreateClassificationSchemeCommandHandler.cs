// <copyright file="CreateClassificationSchemeCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TagManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TagManagement.Domain.Entities.ClassificationSchemes;

namespace NetMetric.CRM.TagManagement.Application.Features.Classifications.Commands.CreateClassificationScheme;

public sealed class CreateClassificationSchemeCommandHandler : IRequestHandler<CreateClassificationSchemeCommand, Guid>
{
    private readonly ITagManagementDbContext _dbContext;

    public CreateClassificationSchemeCommandHandler(ITagManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateClassificationSchemeCommand request, CancellationToken cancellationToken)
    {
        var entity = ClassificationScheme.Create(request.Name, request.EntityType);
        await _dbContext.ClassificationSchemes.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
