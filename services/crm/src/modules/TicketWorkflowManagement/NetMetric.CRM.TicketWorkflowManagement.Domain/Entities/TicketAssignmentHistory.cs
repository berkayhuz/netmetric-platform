// <copyright file="TicketAssignmentHistory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;

public sealed class TicketAssignmentHistory : EntityBase
{
    private TicketAssignmentHistory() { }

    public TicketAssignmentHistory(
        Guid ticketId,
        Guid? previousOwnerUserId,
        Guid? newOwnerUserId,
        Guid? previousQueueId,
        Guid? newQueueId,
        Guid? changedByUserId,
        string? reason,
        DateTime changedAtUtc)
    {
        TicketId = ticketId;
        PreviousOwnerUserId = previousOwnerUserId;
        NewOwnerUserId = newOwnerUserId;
        PreviousQueueId = previousQueueId;
        NewQueueId = newQueueId;
        ChangedByUserId = changedByUserId;
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ChangedAtUtc = changedAtUtc;
    }

    public Guid TicketId { get; private set; }
    public Guid? PreviousOwnerUserId { get; private set; }
    public Guid? NewOwnerUserId { get; private set; }
    public Guid? PreviousQueueId { get; private set; }
    public Guid? NewQueueId { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public string? Reason { get; private set; }
    public DateTime ChangedAtUtc { get; private set; }
}
