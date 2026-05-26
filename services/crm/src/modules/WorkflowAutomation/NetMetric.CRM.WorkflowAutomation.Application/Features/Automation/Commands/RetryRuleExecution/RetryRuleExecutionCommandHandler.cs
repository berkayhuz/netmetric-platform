// <copyright file="RetryRuleExecutionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkflowAutomation.Application.Security;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.RuleExecutionLogs;
using NetMetric.Exceptions;
using NetMetric.Tenancy;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.RetryRuleExecution;

public sealed class RetryRuleExecutionCommandHandler(
    IWorkflowAutomationDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<RetryRuleExecutionCommand, Guid>
{
    public async Task<Guid> Handle(RetryRuleExecutionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var execution = await dbContext.RuleExecutionLogs.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.ExecutionLogId, cancellationToken)
            ?? throw new NotFoundAppException("Workflow execution log not found.");

        if (execution.Status is not (WorkflowExecutionStatuses.Retrying or WorkflowExecutionStatuses.DeadLettered or WorkflowExecutionStatuses.PermissionDenied))
        {
            throw new ConflictAppException("Only retryable, permission-denied or dead-lettered workflow executions can be retried.");
        }

        execution.Requeue(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return execution.Id;
    }
}
