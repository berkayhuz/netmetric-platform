// <copyright file="CreateAssignmentRuleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AssignmentRules;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Assignments.Commands.CreateAssignmentRule;

public sealed class CreateAssignmentRuleCommandHandler(IWorkflowAutomationDbContext dbContext) : IRequestHandler<CreateAssignmentRuleCommand, Guid>
{
    public async Task<Guid> Handle(CreateAssignmentRuleCommand request, CancellationToken cancellationToken)
    {
        var entity = AssignmentRule.Create(
            request.Name,
            request.EntityType,
            request.ConditionJson,
            request.AssigneeSelectorJson,
            fallbackAssigneeJson: request.FallbackAssigneeJson,
            priority: request.Priority);

        await dbContext.AssignmentRules.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
