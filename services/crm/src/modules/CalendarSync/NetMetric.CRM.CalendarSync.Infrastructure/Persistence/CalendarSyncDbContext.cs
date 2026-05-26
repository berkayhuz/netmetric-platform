// <copyright file="CalendarSyncDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CalendarSync.Application.Abstractions.Persistence;
using NetMetric.CRM.CalendarSync.Domain.Entities;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.CalendarSync.Infrastructure.Persistence;

public sealed class CalendarSyncDbContext : AppDbContext, ICalendarSyncDbContext
{
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public CalendarSyncDbContext(
        DbContextOptions<CalendarSyncDbContext> options,
        ITenantContext tenantContext,
        TenantIsolationSaveChangesInterceptor tenantInterceptor,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor) : base(options, tenantContext)
    {
        _tenantInterceptor = tenantInterceptor;
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
    }

    public DbSet<CalendarConnection> Connections => Set<CalendarConnection>();
    public DbSet<CalendarEventBridge> EventBridges => Set<CalendarEventBridge>();
    public DbSet<CalendarSyncRun> SyncRuns => Set<CalendarSyncRun>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CalendarSyncDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
