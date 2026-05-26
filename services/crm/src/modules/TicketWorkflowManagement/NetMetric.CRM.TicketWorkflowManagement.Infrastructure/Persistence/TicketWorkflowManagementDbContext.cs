// <copyright file="TicketWorkflowManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketWorkflowManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.TicketWorkflowManagement.Infrastructure.Persistence;

public sealed class TicketWorkflowManagementDbContext : DbContext, ITicketWorkflowManagementDbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public TicketWorkflowManagementDbContext(
        DbContextOptions<TicketWorkflowManagementDbContext> options,
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

    public DbSet<TicketQueue> TicketQueues => Set<TicketQueue>();
    public DbSet<TicketQueueMembership> TicketQueueMemberships => Set<TicketQueueMembership>();
    public DbSet<TicketAssignmentHistory> TicketAssignmentHistories => Set<TicketAssignmentHistory>();
    public DbSet<TicketStatusHistory> TicketStatusHistories => Set<TicketStatusHistory>();
    public Guid? CurrentTenantId => _tenantProvider.TenantId;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketWorkflowManagementDbContext).Assembly);

        modelBuilder.Entity<TicketQueue>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<TicketQueueMembership>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<TicketAssignmentHistory>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<TicketStatusHistory>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);

        base.OnModelCreating(modelBuilder);
    }
}
