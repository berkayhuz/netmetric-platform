// <copyright file="FinanceOperationsDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.FinanceOperations.Application.Abstractions.Persistence;
using NetMetric.CRM.FinanceOperations.Domain.Entities.Collections;
using NetMetric.CRM.FinanceOperations.Domain.Entities.FinanceHandoffs;
using NetMetric.CRM.FinanceOperations.Domain.Entities.Invoices;
using NetMetric.CRM.FinanceOperations.Domain.Entities.Orders;
using NetMetric.CRM.FinanceOperations.Domain.Entities.Payments;
using NetMetric.CRM.FinanceOperations.Domain.Entities.RevenueReports;
using NetMetric.CRM.FinanceOperations.Infrastructure.Persistence.Configurations;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.FinanceOperations.Infrastructure.Persistence;

public sealed class FinanceOperationsDbContext : AppDbContext, IFinanceOperationsDbContext
{
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public FinanceOperationsDbContext(
        DbContextOptions<FinanceOperationsDbContext> options,
        ITenantContext tenantContext,
        TenantIsolationSaveChangesInterceptor tenantInterceptor,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor) : base(options, tenantContext)
    {
        _tenantInterceptor = tenantInterceptor;
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
    }

    public DbSet<SalesOrder> Orders => Set<SalesOrder>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<PaymentRecord> Payments => Set<PaymentRecord>();
    public DbSet<CollectionCase> Collections => Set<CollectionCase>();
    public DbSet<RevenueSnapshot> RevenueReports => Set<RevenueSnapshot>();
    public DbSet<FinanceHandoff> FinanceHandoffs => Set<FinanceHandoff>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SalesOrderConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentRecordConfiguration());
        modelBuilder.ApplyConfiguration(new CollectionCaseConfiguration());
        modelBuilder.ApplyConfiguration(new RevenueSnapshotConfiguration());
        modelBuilder.ApplyConfiguration(new FinanceHandoffConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
