// <copyright file="IContractLifecycleDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.Contracts;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.Renewals;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.Subscriptions;
using NetMetric.Repository;

namespace NetMetric.CRM.ContractLifecycle.Application.Abstractions.Persistence;

public interface IContractLifecycleDbContext : IUnitOfWork
{
    DbSet<ContractRecord> Contracts { get; }
    DbSet<SubscriptionRecord> Subscriptions { get; }
    DbSet<RenewalTracker> Renewals { get; }
}
