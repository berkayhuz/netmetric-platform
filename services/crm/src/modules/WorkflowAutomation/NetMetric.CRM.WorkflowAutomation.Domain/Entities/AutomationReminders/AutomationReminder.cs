// <copyright file="AutomationReminder.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationReminders;

public sealed class AutomationReminder : AuditableEntity
{
    private AutomationReminder()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid? RelatedEntityId { get; private set; }
    public Guid? AutomationRuleId { get; private set; }
    public Guid? ExecutionLogId { get; private set; }
    public string ReminderType { get; private set; } = "workflow";
    public string RecipientSelectorJson { get; private set; } = "{}";
    public string PayloadJson { get; private set; } = "{}";
    public DateTime DueAtUtc { get; private set; }
    public DateTime? EscalateAtUtc { get; private set; }
    public string Status { get; private set; } = AutomationReminderStatuses.Pending;
    public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;

    public static AutomationReminder Create(
        string name,
        string entityType,
        DateTime dueAtUtc,
        string? reminderType = null,
        string? recipientSelectorJson = null,
        string? payloadJson = null,
        Guid? relatedEntityId = null,
        Guid? automationRuleId = null,
        Guid? executionLogId = null,
        DateTime? escalateAtUtc = null)
    {
        return new AutomationReminder
        {
            Name = Guard.AgainstNullOrWhiteSpace(name),
            EntityType = Guard.AgainstNullOrWhiteSpace(entityType),
            RelatedEntityId = relatedEntityId,
            AutomationRuleId = automationRuleId,
            ExecutionLogId = executionLogId,
            ReminderType = string.IsNullOrWhiteSpace(reminderType) ? "workflow" : reminderType.Trim(),
            RecipientSelectorJson = NormalizeJson(recipientSelectorJson, "{}"),
            PayloadJson = NormalizeJson(payloadJson, "{}"),
            DueAtUtc = dueAtUtc,
            EscalateAtUtc = escalateAtUtc,
            OccurredAtUtc = DateTime.UtcNow
        };
    }

    public void MarkSent(DateTime sentAtUtc)
    {
        Status = AutomationReminderStatuses.Sent;
        OccurredAtUtc = sentAtUtc;
    }

    public void MarkEscalated() => Status = AutomationReminderStatuses.Escalated;

    private static string NormalizeJson(string? json, string fallback)
        => string.IsNullOrWhiteSpace(json) ? fallback : json.Trim();
}

public static class AutomationReminderStatuses
{
    public const string Pending = "pending";
    public const string Sent = "sent";
    public const string Escalated = "escalated";
}
