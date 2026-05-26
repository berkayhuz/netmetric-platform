// <copyright file="ApprovalStep.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.WorkflowAutomation.Domain.Entities.ApprovalWorkflows;
using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.WorkflowAutomation.Domain.Entities.ApprovalSteps;

public sealed class ApprovalStep : AuditableEntity
{
    private ApprovalStep()
    {
    }

    public Guid WorkflowId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid? RelatedEntityId { get; private set; }
    public int Sequence { get; private set; }
    public bool IsRequired { get; private set; }
    public string ApproverSelectorJson { get; private set; } = "{}";
    public string EscalationTargetJson { get; private set; } = "{}";
    public int? DueInMinutes { get; private set; }
    public string Status { get; private set; } = ApprovalStepStatuses.Pending;
    public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;
    public ApprovalWorkflow? Workflow { get; private set; }

    public static ApprovalStep Create(
        Guid workflowId,
        string name,
        string entityType,
        Guid? relatedEntityId,
        int sequence,
        string approverSelectorJson,
        bool isRequired = true,
        int? dueInMinutes = null,
        string? escalationTargetJson = null)
    {
        return new ApprovalStep
        {
            WorkflowId = Guard.AgainstEmpty(workflowId),
            Name = Guard.AgainstNullOrWhiteSpace(name),
            EntityType = Guard.AgainstNullOrWhiteSpace(entityType),
            RelatedEntityId = relatedEntityId,
            Sequence = Math.Max(1, sequence),
            IsRequired = isRequired,
            DueInMinutes = dueInMinutes is null ? null : Math.Max(1, dueInMinutes.Value),
            ApproverSelectorJson = NormalizeJson(approverSelectorJson, "{}"),
            EscalationTargetJson = NormalizeJson(escalationTargetJson, "{}"),
            OccurredAtUtc = DateTime.UtcNow
        };
    }

    public void MarkApproved() => Status = ApprovalStepStatuses.Approved;

    public void MarkRejected() => Status = ApprovalStepStatuses.Rejected;

    public void MarkEscalated() => Status = ApprovalStepStatuses.Escalated;

    private static string NormalizeJson(string? json, string fallback)
        => string.IsNullOrWhiteSpace(json) ? fallback : json.Trim();
}

public static class ApprovalStepStatuses
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Escalated = "escalated";
}
