// <copyright file="WorkflowAutomationPermissions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.WorkflowAutomation.Application.Security;

public static class WorkflowAutomationPermissions
{
    public const string RulesRead = "workflow.rules.read";
    public const string ApprovalsManage = "workflow.approvals.manage";
    public const string AssignmentRulesManage = "workflow.assignment-rules.manage";
    public const string RulesManage = "workflow.rules.manage";
    public const string WebhooksManage = "workflow.webhooks.manage";
    public const string ExecutionsRead = "workflow.executions.read";
}
