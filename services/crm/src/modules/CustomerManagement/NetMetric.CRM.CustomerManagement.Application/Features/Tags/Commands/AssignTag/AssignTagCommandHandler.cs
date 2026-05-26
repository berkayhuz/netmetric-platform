// <copyright file="AssignTagCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.SharedKernel;
using NetMetric.CRM.Tagging;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Tags.Commands.AssignTag;

public sealed class AssignTagCommandHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<AssignTagCommand, Guid>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<Guid> Handle(AssignTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _dbContext.Set<Tag>().FirstOrDefaultAsync(x => x.Id == request.TagId && !x.IsDeleted, cancellationToken)
            ?? throw new NotFoundAppException("Tag not found.");

        var entityName = request.EntityName.Trim().ToLowerInvariant();
        if (!EntityNames.IsSupported(entityName))
        {
            throw new ValidationAppException(
                "Unsupported entity name.",
                new Dictionary<string, string[]>
                {
                    [nameof(request.EntityName)] = ["Entity name must be company, contact or customer."]
                });
        }

        var exists = await _dbContext.Set<TagMap>()
            .AnyAsync(x => !x.IsDeleted
                && x.TagId == request.TagId
                && x.EntityType == entityName
                && x.EntityId == request.EntityId, cancellationToken);

        if (exists)
        {
            return tag.Id;
        }

        var tagMap = new TagMap(tag.Id, new EntityReference(entityName, request.EntityId));

        await _dbContext.Set<TagMap>().AddAsync(tagMap, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return tagMap.Id;
    }
}
