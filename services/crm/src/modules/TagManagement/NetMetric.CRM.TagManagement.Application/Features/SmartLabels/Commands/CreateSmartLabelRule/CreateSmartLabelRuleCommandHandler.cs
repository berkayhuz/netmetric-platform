// <copyright file="CreateSmartLabelRuleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TagManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TagManagement.Domain.Entities.SmartLabelRules;

namespace NetMetric.CRM.TagManagement.Application.Features.SmartLabels.Commands.CreateSmartLabelRule;

public sealed class CreateSmartLabelRuleCommandHandler : IRequestHandler<CreateSmartLabelRuleCommand, Guid>
{
    private readonly ITagManagementDbContext _dbContext;

    public CreateSmartLabelRuleCommandHandler(ITagManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateSmartLabelRuleCommand request, CancellationToken cancellationToken)
    {
        var entity = SmartLabelRule.Create(request.Name, request.EntityType, dataJson: request.ConditionJson);
        await _dbContext.SmartLabelRules.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
