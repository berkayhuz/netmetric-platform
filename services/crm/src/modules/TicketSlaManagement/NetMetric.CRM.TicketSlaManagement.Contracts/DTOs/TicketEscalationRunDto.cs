// <copyright file="TicketEscalationRunDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketSlaManagement.Contracts.DTOs;

public sealed class TicketEscalationRunDto
{
    public Guid Id { get; init; }
    public Guid TicketId { get; init; }
    public Guid EscalationRuleId { get; init; }
    public string MetricType { get; init; } = string.Empty;
    public DateTime ExecutedAtUtc { get; init; }
    public string Note { get; init; } = string.Empty;
}
