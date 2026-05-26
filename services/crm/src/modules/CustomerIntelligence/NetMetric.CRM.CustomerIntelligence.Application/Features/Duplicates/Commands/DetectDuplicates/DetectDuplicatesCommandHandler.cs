// <copyright file="DetectDuplicatesCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.DuplicateMatchs;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Duplicates.Commands.DetectDuplicates;

public sealed class DetectDuplicatesCommandHandler : IRequestHandler<DetectDuplicatesCommand, Guid>
{
    private readonly ICustomerIntelligenceDbContext _dbContext;

    public DetectDuplicatesCommandHandler(ICustomerIntelligenceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(DetectDuplicatesCommand request, CancellationToken cancellationToken)
    {
        var entity = DuplicateMatch.Create("DetectDuplicates");
        await _dbContext.DuplicateMatchs.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
