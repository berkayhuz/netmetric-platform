// <copyright file="SalesForecastingDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Sales;
using NetMetric.CRM.SalesForecasting.Application.Abstractions.Persistence;
using NetMetric.CRM.SalesForecasting.Domain.Entities;
using NetMetric.CRM.SalesForecasting.Infrastructure.Persistence.Configurations;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.SalesForecasting.Infrastructure.Persistence;

public sealed class SalesForecastingDbContext : DbContext, ISalesForecastingDbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public SalesForecastingDbContext(
        DbContextOptions<SalesForecastingDbContext> options,
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

    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<SalesQuota> SalesQuotas => Set<SalesQuota>();
    public DbSet<ForecastAdjustment> ForecastAdjustments => Set<ForecastAdjustment>();
    public DbSet<ForecastSnapshot> ForecastSnapshots => Set<ForecastSnapshot>();
    public Guid? CurrentTenantId => _tenantProvider.TenantId;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SalesQuotaConfiguration());
        modelBuilder.ApplyConfiguration(new ForecastAdjustmentConfiguration());
        modelBuilder.ApplyConfiguration(new ForecastSnapshotConfiguration());
        modelBuilder.ApplyDefaultDecimalPrecision();
        modelBuilder.Entity<Opportunity>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<Deal>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<SalesQuota>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<ForecastAdjustment>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<ForecastSnapshot>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        base.OnModelCreating(modelBuilder);
    }
}
