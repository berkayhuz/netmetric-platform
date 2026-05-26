// <copyright file="IOpportunityManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Activities;
using NetMetric.CRM.Core;
using NetMetric.CRM.Sales;
using NetMetric.Repository;

namespace NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;

public interface IOpportunityManagementDbContext : IUnitOfWork
{
    DbSet<Opportunity> Opportunities { get; }
    DbSet<OpportunityProduct> OpportunityProducts { get; }
    DbSet<OpportunityContact> OpportunityContacts { get; }
    DbSet<OpportunityStageHistory> OpportunityStageHistories { get; }
    DbSet<Quote> Quotes { get; }
    DbSet<QuoteItem> QuoteItems { get; }
    DbSet<LostReason> LostReasons { get; }
    DbSet<Deal> Deals { get; }
    DbSet<Activity> Activities { get; }
    DbSet<GlobalTrashItem> GlobalTrashItems { get; }
}
