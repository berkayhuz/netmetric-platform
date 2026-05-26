// <copyright file="AssignmentRule.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.WorkflowAutomation.Domain.Entities.AssignmentRules;

public sealed class AssignmentRule : AuditableEntity
{
    private AssignmentRule()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid? RelatedEntityId { get; private set; }
    public Guid? AutomationRuleId { get; private set; }
    public string ConditionJson { get; private set; } = "{}";
    public string AssigneeSelectorJson { get; private set; } = "{}";
    public string FallbackAssigneeJson { get; private set; } = "{}";
    public int Priority { get; private set; } = 100;
    public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;

    public static AssignmentRule Create(
        string name,
        string entityType,
        string conditionJson,
        string? assigneeSelectorJson = null,
        Guid? relatedEntityId = null,
        Guid? automationRuleId = null,
        string? fallbackAssigneeJson = null,
        int priority = 100)
    {
        return new AssignmentRule
        {
            Name = Guard.AgainstNullOrWhiteSpace(name),
            EntityType = Guard.AgainstNullOrWhiteSpace(entityType),
            RelatedEntityId = relatedEntityId,
            AutomationRuleId = automationRuleId,
            ConditionJson = NormalizeJson(conditionJson, "{}"),
            AssigneeSelectorJson = NormalizeJson(assigneeSelectorJson, "{}"),
            FallbackAssigneeJson = NormalizeJson(fallbackAssigneeJson, "{}"),
            Priority = Math.Clamp(priority, 0, 1000),
            OccurredAtUtc = DateTime.UtcNow
        };
    }

    private static string NormalizeJson(string? json, string fallback)
        => string.IsNullOrWhiteSpace(json) ? fallback : json.Trim();
}
