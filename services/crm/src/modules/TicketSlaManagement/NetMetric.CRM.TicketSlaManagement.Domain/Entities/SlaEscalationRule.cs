// <copyright file="SlaEscalationRule.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.ServiceManagement;
using NetMetric.CRM.TicketSlaManagement.Domain.Enums;
using NetMetric.Entities;

namespace NetMetric.CRM.TicketSlaManagement.Domain.Entities;

public class SlaEscalationRule : AuditableEntity
{
    private SlaEscalationRule() { }

    public SlaEscalationRule(
        Guid slaPolicyId,
        SlaMetricType metricType,
        int triggerBeforeOrAfterMinutes,
        SlaBreachActionType actionType,
        Guid? targetTeamId,
        Guid? targetUserId,
        bool isEnabled)
    {
        SlaPolicyId = slaPolicyId;
        MetricType = metricType;
        TriggerBeforeOrAfterMinutes = triggerBeforeOrAfterMinutes;
        ActionType = actionType;
        TargetTeamId = targetTeamId;
        TargetUserId = targetUserId;
        IsEnabled = isEnabled;
    }

    public Guid SlaPolicyId { get; private set; }
    public SlaPolicy SlaPolicy { get; private set; } = null!;
    public SlaMetricType MetricType { get; private set; }
    public int TriggerBeforeOrAfterMinutes { get; private set; }
    public SlaBreachActionType ActionType { get; private set; }
    public Guid? TargetTeamId { get; private set; }
    public Guid? TargetUserId { get; private set; }
    public bool IsEnabled { get; private set; }

    public void Toggle(bool enabled) => IsEnabled = enabled;
}
