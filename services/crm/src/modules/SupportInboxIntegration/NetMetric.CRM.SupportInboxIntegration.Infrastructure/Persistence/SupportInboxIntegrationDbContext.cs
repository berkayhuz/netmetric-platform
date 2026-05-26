// <copyright file="SupportInboxIntegrationDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Persistence;
using NetMetric.CRM.SupportInboxIntegration.Domain.Entities;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.SupportInboxIntegration.Infrastructure.Persistence;

public sealed class SupportInboxIntegrationDbContext : AppDbContext, ISupportInboxIntegrationDbContext
{
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public SupportInboxIntegrationDbContext(
        DbContextOptions<SupportInboxIntegrationDbContext> options,
        ITenantContext tenantContext,
        TenantIsolationSaveChangesInterceptor tenantInterceptor,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor) : base(options, tenantContext)
    {
        _tenantInterceptor = tenantInterceptor;
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
    }

    public DbSet<SupportInboxConnection> Connections => Set<SupportInboxConnection>();
    public DbSet<SupportInboxRule> Rules => Set<SupportInboxRule>();
    public DbSet<SupportInboxMessage> Messages => Set<SupportInboxMessage>();
    public DbSet<SupportInboxSyncRun> SyncRuns => Set<SupportInboxSyncRun>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupportInboxIntegrationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
