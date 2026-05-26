// <copyright file="ITicketManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ServiceManagement;
using NetMetric.CRM.Support;

namespace NetMetric.CRM.TicketManagement.Application.Abstractions.Persistence;

public interface ITicketManagementDbContext
{
    DbSet<Ticket> Tickets { get; }
    DbSet<TicketComment> TicketComments { get; }
    DbSet<TicketCategory> TicketCategories { get; }
    DbSet<TicketStatusHistory> TicketStatusHistories { get; }
    DbSet<TicketPriorityHistory> TicketPriorityHistories { get; }
    DbSet<SlaPolicy> SlaPolicies { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
