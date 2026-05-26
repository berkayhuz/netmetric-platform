// <copyright file="TicketSlaInstance.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.ServiceManagement;
using NetMetric.Entities;

namespace NetMetric.CRM.TicketSlaManagement.Domain.Entities;

public class TicketSlaInstance : AuditableEntity
{
    private TicketSlaInstance() { }

    public TicketSlaInstance(
        Guid ticketId,
        Guid slaPolicyId,
        DateTime firstResponseDueAtUtc,
        DateTime resolutionDueAtUtc)
    {
        TicketId = ticketId;
        SlaPolicyId = slaPolicyId;
        FirstResponseDueAtUtc = firstResponseDueAtUtc;
        ResolutionDueAtUtc = resolutionDueAtUtc;
    }

    public Guid TicketId { get; private set; }
    public Guid SlaPolicyId { get; private set; }
    public SlaPolicy SlaPolicy { get; private set; } = null!;
    public DateTime FirstResponseDueAtUtc { get; private set; }
    public DateTime ResolutionDueAtUtc { get; private set; }
    public DateTime? FirstRespondedAtUtc { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }
    public bool IsFirstResponseBreached { get; private set; }
    public bool IsResolutionBreached { get; private set; }

    public void MarkFirstResponse(DateTime respondedAtUtc)
    {
        FirstRespondedAtUtc = respondedAtUtc;
        IsFirstResponseBreached = respondedAtUtc > FirstResponseDueAtUtc;
    }

    public void MarkResolved(DateTime resolvedAtUtc)
    {
        ResolvedAtUtc = resolvedAtUtc;
        IsResolutionBreached = resolvedAtUtc > ResolutionDueAtUtc;
    }

    public void Evaluate(DateTime utcNow)
    {
        if (FirstRespondedAtUtc is null && utcNow > FirstResponseDueAtUtc)
            IsFirstResponseBreached = true;

        if (ResolvedAtUtc is null && utcNow > ResolutionDueAtUtc)
            IsResolutionBreached = true;
    }
}
