// <copyright file="DeactivateAutomationRuleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkflowAutomation.Application.Security;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;
using NetMetric.Tenancy;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.DeactivateAutomationRule;

public sealed class DeactivateAutomationRuleCommandHandler(
    IWorkflowAutomationDbContext dbContext,
    ITenantContext tenantContext,
    ICurrentUserService currentUserService) : IRequestHandler<DeactivateAutomationRuleCommand>
{
    public async Task Handle(DeactivateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var rule = await dbContext.AutomationRules.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.RuleId, cancellationToken)
            ?? throw new NotFoundAppException("Workflow automation rule not found.");

        rule.Deactivate(DateTime.UtcNow, currentUserService.UserName);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
