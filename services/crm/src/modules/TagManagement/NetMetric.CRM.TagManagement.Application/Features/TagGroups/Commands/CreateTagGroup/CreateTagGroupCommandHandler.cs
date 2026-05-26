// <copyright file="CreateTagGroupCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using MediatR;
using NetMetric.CRM.TagManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TagManagement.Domain.Entities.TagGroups;

namespace NetMetric.CRM.TagManagement.Application.Features.TagGroups.Commands.CreateTagGroup;

public sealed class CreateTagGroupCommandHandler : IRequestHandler<CreateTagGroupCommand, Guid>
{
    private readonly ITagManagementDbContext _dbContext;

    public CreateTagGroupCommandHandler(ITagManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateTagGroupCommand request, CancellationToken cancellationToken)
    {
        var dataJson = request.Color is null ? null : JsonSerializer.Serialize(new { request.Color });
        var entity = TagGroup.Create(request.Name, dataJson: dataJson);
        await _dbContext.TagGroups.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
