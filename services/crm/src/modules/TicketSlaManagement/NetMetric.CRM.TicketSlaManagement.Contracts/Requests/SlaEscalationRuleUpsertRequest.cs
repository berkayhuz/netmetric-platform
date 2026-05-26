// <copyright file="SlaEscalationRuleUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketSlaManagement.Contracts.Requests;

public sealed class SlaEscalationRuleUpsertRequest
{
    public Guid SlaPolicyId { get; init; }
    public string MetricType { get; init; } = string.Empty;
    public int TriggerBeforeOrAfterMinutes { get; init; }
    public string ActionType { get; init; } = string.Empty;
    public Guid? TargetTeamId { get; init; }
    public Guid? TargetUserId { get; init; }
    public bool IsEnabled { get; init; }
}
