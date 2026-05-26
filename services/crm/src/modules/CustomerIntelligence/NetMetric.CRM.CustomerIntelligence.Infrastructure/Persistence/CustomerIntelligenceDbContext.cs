// <copyright file="CustomerIntelligenceDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
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
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.CustomerIntelligence.Infrastructure.Persistence;

public sealed class CustomerIntelligenceDbContext : AppDbContext, ICustomerIntelligenceDbContext
{
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;

    public CustomerIntelligenceDbContext(
        DbContextOptions<CustomerIntelligenceDbContext> options,
        ITenantContext tenantContext,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor,
        TenantIsolationSaveChangesInterceptor tenantInterceptor) : base(options, tenantContext)
    {
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
        _tenantInterceptor = tenantInterceptor;
    }

    public DbSet<DuplicateMatch> DuplicateMatchs => Set<DuplicateMatch>();
    public DbSet<MergeJob> MergeJobs => Set<MergeJob>();
    public DbSet<AccountHierarchyNode> AccountHierarchyNodes => Set<AccountHierarchyNode>();
    public DbSet<CustomerTimelineEntry> CustomerTimelineEntrys => Set<CustomerTimelineEntry>();
    public DbSet<SavedView> SavedViews => Set<SavedView>();
    public DbSet<OwnershipHistoryEntry> OwnershipHistoryEntrys => Set<OwnershipHistoryEntry>();
    public DbSet<CustomerRelationship> CustomerRelationships => Set<CustomerRelationship>();
    public DbSet<CustomerHealthScore> CustomerHealthScores => Set<CustomerHealthScore>();
    public DbSet<BehavioralEvent> BehavioralEvents => Set<BehavioralEvent>();
    public DbSet<IdentityProfile> IdentityProfiles => Set<IdentityProfile>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerIntelligenceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
