// <copyright file="ApprovalWorkflow.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.WorkflowAutomation.Domain.Entities.ApprovalSteps;
using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.WorkflowAutomation.Domain.Entities.ApprovalWorkflows;

public sealed class ApprovalWorkflow : AuditableEntity
{
    private readonly List<ApprovalStep> _steps = [];

    private ApprovalWorkflow()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid? RelatedEntityId { get; private set; }
    public Guid? AutomationRuleId { get; private set; }
    public int Version { get; private set; } = 1;
    public string RoutingPolicyJson { get; private set; } = "{}";
    public string EscalationPolicyJson { get; private set; } = "{}";
    public string SlaPolicyJson { get; private set; } = "{}";
    public string Status { get; private set; } = ApprovalWorkflowStatuses.Draft;
    public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;
    public IReadOnlyCollection<ApprovalStep> Steps => _steps;

    public static ApprovalWorkflow Create(
        string name,
        string entityType,
        Guid? relatedEntityId = null,
        Guid? automationRuleId = null,
        string? routingPolicyJson = null,
        string? escalationPolicyJson = null,
        string? slaPolicyJson = null)
    {
        return new ApprovalWorkflow
        {
            Name = Guard.AgainstNullOrWhiteSpace(name),
            EntityType = Guard.AgainstNullOrWhiteSpace(entityType),
            RelatedEntityId = relatedEntityId,
            AutomationRuleId = automationRuleId,
            RoutingPolicyJson = NormalizeJson(routingPolicyJson, "{}"),
            EscalationPolicyJson = NormalizeJson(escalationPolicyJson, "{}"),
            SlaPolicyJson = NormalizeJson(slaPolicyJson, "{}"),
            OccurredAtUtc = DateTime.UtcNow
        };
    }

    public ApprovalStep AddStep(
        string name,
        int sequence,
        string approverSelectorJson,
        bool isRequired = true,
        int? dueInMinutes = null,
        string? escalationTargetJson = null)
    {
        var step = ApprovalStep.Create(
            Id,
            name,
            EntityType,
            RelatedEntityId,
            sequence,
            approverSelectorJson,
            isRequired,
            dueInMinutes,
            escalationTargetJson);
        _steps.Add(step);
        return step;
    }

    public new void Activate()
    {
        Status = ApprovalWorkflowStatuses.Active;
        SetActive(true);
    }

    public void Complete()
    {
        Status = ApprovalWorkflowStatuses.Completed;
        SetActive(false);
    }

    public void Escalate()
    {
        Status = ApprovalWorkflowStatuses.Escalated;
    }

    private static string NormalizeJson(string? json, string fallback)
        => string.IsNullOrWhiteSpace(json) ? fallback : json.Trim();
}

public static class ApprovalWorkflowStatuses
{
    public const string Draft = "draft";
    public const string Active = "active";
    public const string Completed = "completed";
    public const string Escalated = "escalated";
}
