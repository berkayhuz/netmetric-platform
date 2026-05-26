// <copyright file="TicketStatusHistory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;

public sealed class TicketStatusHistory : EntityBase
{
    private TicketStatusHistory() { }

    public TicketStatusHistory(Guid ticketId, string previousStatus, string newStatus, Guid? changedByUserId, string? note, DateTime changedAtUtc)
    {
        TicketId = ticketId;
        PreviousStatus = Guard.AgainstNullOrWhiteSpace(previousStatus);
        NewStatus = Guard.AgainstNullOrWhiteSpace(newStatus);
        ChangedByUserId = changedByUserId;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        ChangedAtUtc = changedAtUtc;
    }

    public Guid TicketId { get; private set; }
    public string PreviousStatus { get; private set; } = null!;
    public string NewStatus { get; private set; } = null!;
    public Guid? ChangedByUserId { get; private set; }
    public string? Note { get; private set; }
    public DateTime ChangedAtUtc { get; private set; }
}
