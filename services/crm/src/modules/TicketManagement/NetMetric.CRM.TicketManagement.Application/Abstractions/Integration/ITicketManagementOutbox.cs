// <copyright file="ITicketManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Support;

namespace NetMetric.CRM.TicketManagement.Application.Abstractions.Integration;

public interface ITicketManagementOutbox
{
    Task EnqueueTicketCreatedAsync(Ticket ticket, CancellationToken cancellationToken);

    Task EnqueueTicketUpdatedAsync(Ticket ticket, CancellationToken cancellationToken);

    Task EnqueueTicketDeletedAsync(Ticket ticket, CancellationToken cancellationToken);

    Task EnqueueTicketRestoredAsync(Ticket ticket, CancellationToken cancellationToken);

    Task EnqueueTicketPurgedAsync(Guid tenantId, Guid ticketId, string? subject, Guid? assigneeUserId, CancellationToken cancellationToken);
}
