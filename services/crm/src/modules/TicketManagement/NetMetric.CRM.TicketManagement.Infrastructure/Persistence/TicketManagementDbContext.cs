// <copyright file="TicketManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.ServiceManagement;
using NetMetric.CRM.Support;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketManagement.Infrastructure.Outbox;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.TicketManagement.Infrastructure.Persistence;

public sealed class TicketManagementDbContext : DbContext, ITicketManagementDbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public TicketManagementDbContext(
        DbContextOptions<TicketManagementDbContext> options,
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

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<TicketStatusHistory> TicketStatusHistories => Set<TicketStatusHistory>();
    public DbSet<TicketPriorityHistory> TicketPriorityHistories => Set<TicketPriorityHistory>();
    public DbSet<SlaPolicy> SlaPolicies => Set<SlaPolicy>();
    public DbSet<GlobalTrashItem> GlobalTrashItems => Set<GlobalTrashItem>();
    public DbSet<TicketManagementOutboxMessage> OutboxMessages => Set<TicketManagementOutboxMessage>();
    public Guid? CurrentTenantId => _tenantProvider.TenantId;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketManagementDbContext).Assembly);
        modelBuilder.Entity<Ticket>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<TicketComment>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<TicketCategory>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<TicketStatusHistory>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<TicketPriorityHistory>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<SlaPolicy>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<GlobalTrashItem>().HasQueryFilter(x => CurrentTenantId.HasValue && x.TenantId == CurrentTenantId.Value);
        base.OnModelCreating(modelBuilder);
    }
}
