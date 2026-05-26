// <copyright file="TicketEscalationRun.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.TicketSlaManagement.Domain.Enums;
using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.TicketSlaManagement.Domain.Entities;

public class TicketEscalationRun : AuditableEntity
{
    private TicketEscalationRun() { }

    public TicketEscalationRun(
        Guid ticketId,
        Guid ticketSlaInstanceId,
        Guid escalationRuleId,
        SlaMetricType metricType,
        string note)
    {
        TicketId = ticketId;
        TicketSlaInstanceId = ticketSlaInstanceId;
        EscalationRuleId = escalationRuleId;
        MetricType = metricType;
        Note = Guard.AgainstNullOrWhiteSpace(note);
        ExecutedAtUtc = DateTime.UtcNow;
    }

    public Guid TicketId { get; private set; }
    public Guid TicketSlaInstanceId { get; private set; }
    public Guid EscalationRuleId { get; private set; }
    public SlaMetricType MetricType { get; private set; }
    public DateTime ExecutedAtUtc { get; private set; }
    public string Note { get; private set; } = null!;
}
