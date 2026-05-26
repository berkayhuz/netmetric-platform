// <copyright file="ListTagsForEntityQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Tags;
using NetMetric.CRM.Tagging;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Tags.Queries.ListTagsForEntity;

public sealed class ListTagsForEntityQueryHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<ListTagsForEntityQuery, IReadOnlyList<TagDto>>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<TagDto>> Handle(ListTagsForEntityQuery request, CancellationToken cancellationToken)
    {
        var entityName = request.EntityName.Trim().ToLowerInvariant();
        var maps = _dbContext.Set<TagMap>()
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .AsQueryable();

        maps = entityName switch
        {
            EntityNames.Company => maps.Where(x => x.EntityType == EntityNames.Company && x.EntityId == request.EntityId),
            EntityNames.Contact => maps.Where(x => x.EntityType == EntityNames.Contact && x.EntityId == request.EntityId),
            EntityNames.Customer => maps.Where(x => x.EntityType == EntityNames.Customer && x.EntityId == request.EntityId),
            _ => maps.Where(_ => false)
        };

        return await maps
            .Join(_dbContext.Set<Tag>().AsNoTracking().Where(x => !x.IsDeleted),
                map => map.TagId,
                tag => tag.Id,
                (map, tag) => new TagDto
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    ColorHex = tag.ColorHex,
                    Description = tag.Description
                })
            .Distinct()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
