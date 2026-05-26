// <copyright file="UnassignTagCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Common;
using NetMetric.CRM.Tagging;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Tags.Commands.UnassignTag;

public sealed class UnassignTagCommandHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<UnassignTagCommand, Unit>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<Unit> Handle(UnassignTagCommand request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<TagMap>().Where(x => !x.IsDeleted && x.TagId == request.TagId).AsQueryable();

        query = request.EntityName.Trim().ToLowerInvariant() switch
        {
            EntityNames.Company => query.Where(x => x.EntityType == EntityNames.Company && x.EntityId == request.EntityId),
            EntityNames.Contact => query.Where(x => x.EntityType == EntityNames.Contact && x.EntityId == request.EntityId),
            EntityNames.Customer => query.Where(x => x.EntityType == EntityNames.Customer && x.EntityId == request.EntityId),
            _ => throw new ValidationAppException(
                "Unsupported entity name.",
                new Dictionary<string, string[]>
                {
                    [nameof(request.EntityName)] = ["Entity name must be company, contact or customer."]
                })
        };

        var mappings = await query.ToListAsync(cancellationToken);
        _dbContext.Set<TagMap>().RemoveRange(mappings);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
