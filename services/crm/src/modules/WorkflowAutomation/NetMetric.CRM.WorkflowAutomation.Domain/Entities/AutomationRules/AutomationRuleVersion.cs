// <copyright file="AutomationRuleVersion.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationRules;

public sealed class AutomationRuleVersion : AuditableEntity
{
    private AutomationRuleVersion()
    {
    }

    public AutomationRuleVersion(
        Guid tenantId,
        Guid ruleId,
        int version,
        string triggerDefinitionJson,
        string conditionDefinitionJson,
        string actionDefinitionJson,
        string changeReason,
        string? changedBy)
    {
        TenantId = tenantId;
        RuleId = Guard.AgainstEmpty(ruleId);
        Version = Math.Max(1, version);
        TriggerDefinitionJson = string.IsNullOrWhiteSpace(triggerDefinitionJson) ? "{}" : triggerDefinitionJson.Trim();
        ConditionDefinitionJson = string.IsNullOrWhiteSpace(conditionDefinitionJson) ? "{}" : conditionDefinitionJson.Trim();
        ActionDefinitionJson = string.IsNullOrWhiteSpace(actionDefinitionJson) ? "[]" : actionDefinitionJson.Trim();
        ChangeReason = Guard.AgainstNullOrWhiteSpace(changeReason);
        ChangedBy = string.IsNullOrWhiteSpace(changedBy) ? null : changedBy.Trim();
    }

    public Guid RuleId { get; private set; }
    public int Version { get; private set; }
    public string TriggerDefinitionJson { get; private set; } = "{}";
    public string ConditionDefinitionJson { get; private set; } = "{}";
    public string ActionDefinitionJson { get; private set; } = "[]";
    public string ChangeReason { get; private set; } = string.Empty;
    public string? ChangedBy { get; private set; }
    public AutomationRule? Rule { get; private set; }
}
