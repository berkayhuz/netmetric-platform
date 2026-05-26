// <copyright file="ICustomerIntelligenceDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.AccountHierarchyNodes;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.BehavioralEvents;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerHealthScores;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerRelationships;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerTimelineEntrys;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.DuplicateMatchs;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.IdentityProfiles;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.MergeJobs;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.OwnershipHistoryEntrys;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.SavedViews;
using NetMetric.Repository;

namespace NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;

public interface ICustomerIntelligenceDbContext : IUnitOfWork
{
    DbSet<DuplicateMatch> DuplicateMatchs { get; }
    DbSet<MergeJob> MergeJobs { get; }
    DbSet<AccountHierarchyNode> AccountHierarchyNodes { get; }
    DbSet<CustomerTimelineEntry> CustomerTimelineEntrys { get; }
    DbSet<SavedView> SavedViews { get; }
    DbSet<OwnershipHistoryEntry> OwnershipHistoryEntrys { get; }
    DbSet<CustomerRelationship> CustomerRelationships { get; }
    DbSet<CustomerHealthScore> CustomerHealthScores { get; }
    DbSet<BehavioralEvent> BehavioralEvents { get; }
    DbSet<IdentityProfile> IdentityProfiles { get; }
}
