// <copyright file="IWorkflowAutomationDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.ApprovalSteps;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.ApprovalWorkflows;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AssignmentRules;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationReminders;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationRules;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.RuleExecutionLogs;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.WebhookSubscriptions;
using NetMetric.Repository;

namespace NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Persistence;

public interface IWorkflowAutomationDbContext : IUnitOfWork
{
    DbSet<ApprovalWorkflow> ApprovalWorkflows { get; }
    DbSet<ApprovalStep> ApprovalSteps { get; }
    DbSet<AssignmentRule> AssignmentRules { get; }
    DbSet<AutomationRule> AutomationRules { get; }
    DbSet<AutomationRuleVersion> AutomationRuleVersions { get; }
    DbSet<AutomationReminder> AutomationReminders { get; }
    DbSet<WebhookSubscription> WebhookSubscriptions { get; }
    DbSet<WebhookDeliveryLog> WebhookDeliveryLogs { get; }
    DbSet<RuleExecutionLog> RuleExecutionLogs { get; }
}
