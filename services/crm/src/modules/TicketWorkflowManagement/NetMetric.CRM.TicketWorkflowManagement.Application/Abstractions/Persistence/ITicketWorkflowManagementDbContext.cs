// <copyright file="ITicketWorkflowManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;
using NetMetric.Repository;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Abstractions.Persistence;

public interface ITicketWorkflowManagementDbContext : IUnitOfWork
{
    DbSet<TicketQueue> TicketQueues { get; }
    DbSet<TicketQueueMembership> TicketQueueMemberships { get; }
    DbSet<TicketAssignmentHistory> TicketAssignmentHistories { get; }
    DbSet<TicketStatusHistory> TicketStatusHistories { get; }
}
