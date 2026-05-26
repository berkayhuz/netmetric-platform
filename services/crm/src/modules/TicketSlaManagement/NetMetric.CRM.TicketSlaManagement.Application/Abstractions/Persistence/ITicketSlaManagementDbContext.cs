// <copyright file="ITicketSlaManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ServiceManagement;
using NetMetric.CRM.TicketSlaManagement.Domain.Entities;
using NetMetric.Repository;

namespace NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Persistence;

public interface ITicketSlaManagementDbContext : IUnitOfWork
{
    DbSet<SlaPolicy> SlaPolicies { get; }
    DbSet<SlaEscalationRule> SlaEscalationRules { get; }
    DbSet<TicketSlaInstance> TicketSlaInstances { get; }
    DbSet<TicketEscalationRun> TicketEscalationRuns { get; }
}
