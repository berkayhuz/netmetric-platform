// <copyright file="MergeEntitiesCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.DuplicateMatchs;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Merges.Commands.MergeEntities;

public sealed class MergeEntitiesCommandHandler : IRequestHandler<MergeEntitiesCommand, Guid>
{
    private readonly ICustomerIntelligenceDbContext _dbContext;

    public MergeEntitiesCommandHandler(ICustomerIntelligenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(MergeEntitiesCommand request, CancellationToken cancellationToken)
    {
        var entity = DuplicateMatch.Create("MergeEntities");
        await _dbContext.DuplicateMatchs.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
