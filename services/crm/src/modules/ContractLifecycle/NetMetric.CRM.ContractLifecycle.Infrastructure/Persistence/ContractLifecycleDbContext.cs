// <copyright file="ContractLifecycleDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ContractLifecycle.Application.Abstractions.Persistence;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.ChurnPrevention;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.ContractDocuments;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.Contracts;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.Renewals;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.Subscriptions;
using NetMetric.CRM.ContractLifecycle.Infrastructure.Persistence.Configurations;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.ContractLifecycle.Infrastructure.Persistence;

public sealed class ContractLifecycleDbContext : AppDbContext, IContractLifecycleDbContext
{
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public ContractLifecycleDbContext(
        DbContextOptions<ContractLifecycleDbContext> options,
        ITenantContext tenantContext,
        TenantIsolationSaveChangesInterceptor tenantInterceptor,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor) : base(options, tenantContext)
    {
        _tenantInterceptor = tenantInterceptor;
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
    }

    public DbSet<ContractRecord> Contracts => Set<ContractRecord>();
    public DbSet<SubscriptionRecord> Subscriptions => Set<SubscriptionRecord>();
    public DbSet<RenewalTracker> Renewals => Set<RenewalTracker>();
    public DbSet<ChurnPreventionCase> ChurnPrevention => Set<ChurnPreventionCase>();
    public DbSet<ContractDocument> ContractDocuments => Set<ContractDocument>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ContractRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SubscriptionRecordConfiguration());
        modelBuilder.ApplyConfiguration(new RenewalTrackerConfiguration());
        modelBuilder.ApplyConfiguration(new ChurnPreventionCaseConfiguration());
        modelBuilder.ApplyConfiguration(new ContractDocumentConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
