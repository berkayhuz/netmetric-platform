// <copyright file="AutomationRule.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationRules;

public sealed class AutomationRule : AuditableEntity
{
    private readonly List<AutomationRuleVersion> _versions = [];

    private AutomationRule()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string TriggerType { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string TriggerDefinitionJson { get; private set; } = "{}";
    public string ConditionDefinitionJson { get; private set; } = "{}";
    public string ActionDefinitionJson { get; private set; } = "[]";
    public int Version { get; private set; } = 1;
    public int Priority { get; private set; } = 100;
    public int MaxAttempts { get; private set; } = 3;
    public int TenantDailyExecutionLimit { get; private set; } = 1000;
    public int LoopPreventionWindowSeconds { get; private set; } = 300;
    public int MaxLoopDepth { get; private set; } = 4;
    public DateTime? ActivatedAtUtc { get; private set; }
    public DateTime? DeactivatedAtUtc { get; private set; }
    public string? ActivationChangedBy { get; private set; }
    public DateTime? LastTriggeredAtUtc { get; private set; }
    public string? LastExecutionStatus { get; private set; }
    public DateTime? NextRunAtUtc { get; private set; }
    public string? ScheduleCron { get; private set; }
    public int? ScheduleIntervalSeconds { get; private set; }
    public string ErrorPolicy { get; private set; } = WorkflowRuleErrorPolicies.RetryThenDeadLetter;
    public string? TemplateKey { get; private set; }
    public IReadOnlyCollection<AutomationRuleVersion> Versions => _versions;

    public static AutomationRule Create(
        string name,
        string triggerType,
        string entityType,
        string triggerDefinitionJson,
        string conditionDefinitionJson,
        string actionDefinitionJson,
        string? description = null,
        int priority = 100,
        int maxAttempts = 3,
        int tenantDailyExecutionLimit = 1000,
        int loopPreventionWindowSeconds = 300,
        int maxLoopDepth = 4,
        bool isActive = false,
        DateTime? nextRunAtUtc = null,
        string? scheduleCron = null,
        int? scheduleIntervalSeconds = null,
        string? templateKey = null,
        string? changedBy = null)
    {
        var rule = new AutomationRule
        {
            Name = Guard.AgainstNullOrWhiteSpace(name),
            Description = NormalizeOptional(description),
            TriggerType = Guard.AgainstNullOrWhiteSpace(triggerType),
            EntityType = Guard.AgainstNullOrWhiteSpace(entityType),
            TriggerDefinitionJson = NormalizeJson(triggerDefinitionJson, "{}"),
            ConditionDefinitionJson = NormalizeJson(conditionDefinitionJson, "{}"),
            ActionDefinitionJson = NormalizeJson(actionDefinitionJson, "[]"),
            Priority = Math.Clamp(priority, 0, 1000),
            MaxAttempts = Math.Clamp(maxAttempts, 1, 10),
            TenantDailyExecutionLimit = Math.Clamp(tenantDailyExecutionLimit, 1, 100_000),
            LoopPreventionWindowSeconds = Math.Clamp(loopPreventionWindowSeconds, 30, 86_400),
            MaxLoopDepth = Math.Clamp(maxLoopDepth, 1, 25),
            NextRunAtUtc = nextRunAtUtc,
            ScheduleCron = NormalizeOptional(scheduleCron),
            ScheduleIntervalSeconds = scheduleIntervalSeconds is null ? null : Math.Max(30, scheduleIntervalSeconds.Value),
            TemplateKey = NormalizeOptional(templateKey)
        };

        rule.SetActive(isActive);
        if (isActive)
        {
            rule.ActivatedAtUtc = DateTime.UtcNow;
            rule.ActivationChangedBy = NormalizeOptional(changedBy);
        }

        rule.RecordVersion("initial", changedBy);
        return rule;
    }

    public void UpdateDefinition(
        string name,
        string triggerType,
        string entityType,
        string triggerDefinitionJson,
        string conditionDefinitionJson,
        string actionDefinitionJson,
        string? description,
        int priority,
        int maxAttempts,
        int tenantDailyExecutionLimit,
        int loopPreventionWindowSeconds,
        int maxLoopDepth,
        DateTime? nextRunAtUtc,
        string? scheduleCron,
        int? scheduleIntervalSeconds,
        string? templateKey,
        string? changedBy)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = NormalizeOptional(description);
        TriggerType = Guard.AgainstNullOrWhiteSpace(triggerType);
        EntityType = Guard.AgainstNullOrWhiteSpace(entityType);
        TriggerDefinitionJson = NormalizeJson(triggerDefinitionJson, "{}");
        ConditionDefinitionJson = NormalizeJson(conditionDefinitionJson, "{}");
        ActionDefinitionJson = NormalizeJson(actionDefinitionJson, "[]");
        Priority = Math.Clamp(priority, 0, 1000);
        MaxAttempts = Math.Clamp(maxAttempts, 1, 10);
        TenantDailyExecutionLimit = Math.Clamp(tenantDailyExecutionLimit, 1, 100_000);
        LoopPreventionWindowSeconds = Math.Clamp(loopPreventionWindowSeconds, 30, 86_400);
        MaxLoopDepth = Math.Clamp(maxLoopDepth, 1, 25);
        NextRunAtUtc = nextRunAtUtc;
        ScheduleCron = NormalizeOptional(scheduleCron);
        ScheduleIntervalSeconds = scheduleIntervalSeconds is null ? null : Math.Max(30, scheduleIntervalSeconds.Value);
        TemplateKey = NormalizeOptional(templateKey);
        Version += 1;
        RecordVersion("definition-updated", changedBy);
    }

    public new void Activate() => Activate(DateTime.UtcNow, null);

    public void Activate(DateTime activatedAtUtc, string? changedBy)
    {
        SetActive(true);
        ActivatedAtUtc = activatedAtUtc;
        DeactivatedAtUtc = null;
        ActivationChangedBy = NormalizeOptional(changedBy);
    }

    public new void Deactivate() => Deactivate(DateTime.UtcNow, null);

    public void Deactivate(DateTime deactivatedAtUtc, string? changedBy)
    {
        SetActive(false);
        DeactivatedAtUtc = deactivatedAtUtc;
        ActivationChangedBy = NormalizeOptional(changedBy);
    }

    public void MarkTriggered(DateTime triggeredAtUtc, string status)
    {
        LastTriggeredAtUtc = triggeredAtUtc;
        LastExecutionStatus = Guard.AgainstNullOrWhiteSpace(status);
    }

    public void ScheduleNext(DateTime? nextRunAtUtc)
    {
        NextRunAtUtc = nextRunAtUtc;
    }

    private void RecordVersion(string reason, string? changedBy)
    {
        _versions.Add(new AutomationRuleVersion(
            TenantId,
            Id,
            Version,
            TriggerDefinitionJson,
            ConditionDefinitionJson,
            ActionDefinitionJson,
            Guard.AgainstNullOrWhiteSpace(reason),
            changedBy));
    }

    private static string NormalizeJson(string? json, string fallback)
        => string.IsNullOrWhiteSpace(json) ? fallback : json.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public static class WorkflowRuleErrorPolicies
{
    public const string RetryThenDeadLetter = "retry-then-dead-letter";
    public const string Stop = "stop";
}
