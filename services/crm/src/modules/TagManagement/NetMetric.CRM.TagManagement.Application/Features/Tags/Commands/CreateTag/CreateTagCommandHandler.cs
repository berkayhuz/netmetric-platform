// <copyright file="CreateTagCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using MediatR;
using NetMetric.CRM.TagManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TagManagement.Domain.Entities.TagDefinitions;

namespace NetMetric.CRM.TagManagement.Application.Features.Tags.Commands.CreateTag;

public sealed class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Guid>
{
    private readonly ITagManagementDbContext _dbContext;

    public CreateTagCommandHandler(ITagManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var entity = TagDefinition.Create(request.Name, dataJson: JsonSerializer.Serialize(new { request.Color }));
        await _dbContext.TagDefinitions.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
