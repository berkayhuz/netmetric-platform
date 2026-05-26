// <copyright file="DeleteTagCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Tagging;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Tags.Commands.DeleteTag;

public sealed class DeleteTagCommandHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<DeleteTagCommand, Unit>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<Unit> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _dbContext.Set<Tag>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.TagId, cancellationToken)
            ?? throw new NotFoundAppException("Tag not found.");

        var mappings = await _dbContext.Set<TagMap>()
            .Where(x => !x.IsDeleted && x.TagId == request.TagId)
            .ToListAsync(cancellationToken);

        _dbContext.Set<TagMap>().RemoveRange(mappings);
        _dbContext.Set<Tag>().Remove(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
