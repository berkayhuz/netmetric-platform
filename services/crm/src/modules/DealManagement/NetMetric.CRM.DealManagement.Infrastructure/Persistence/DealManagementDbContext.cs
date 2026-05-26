// <copyright file="DealManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Domain.Entities;
using NetMetric.CRM.DealManagement.Infrastructure.Outbox;
using NetMetric.CRM.DealManagement.Infrastructure.Persistence.Configurations;
using NetMetric.CRM.Sales;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.DealManagement.Infrastructure.Persistence;

public sealed class DealManagementDbContext : DbContext, IDealManagementDbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public DealManagementDbContext(
        DbContextOptions<DealManagementDbContext> options,
        ITenantProvider tenantProvider,
        TenantIsolationSaveChangesInterceptor tenantInterceptor,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor) : base(options)
    {
        _tenantProvider = tenantProvider;
        _tenantInterceptor = tenantInterceptor;
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
    }

    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<LostReason> LostReasons => Set<LostReason>();
    public DbSet<DealOutcomeHistory> DealOutcomeHistories => Set<DealOutcomeHistory>();
    public DbSet<WinLossReview> WinLossReviews => Set<WinLossReview>();
    public DbSet<GlobalTrashItem> GlobalTrashItems => Set<GlobalTrashItem>();
    public DbSet<DealManagementOutboxMessage> OutboxMessages => Set<DealManagementOutboxMessage>();
    public Guid? CurrentTenantId => _tenantProvider.TenantId;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DealOutcomeHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new WinLossReviewConfiguration());
        modelBuilder.ApplyConfiguration(new DealManagementOutboxMessageConfiguration());
        modelBuilder.ApplyDefaultDecimalPrecision();
        modelBuilder.Entity<Deal>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<LostReason>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<DealOutcomeHistory>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<WinLossReview>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        base.OnModelCreating(modelBuilder);
    }
}
