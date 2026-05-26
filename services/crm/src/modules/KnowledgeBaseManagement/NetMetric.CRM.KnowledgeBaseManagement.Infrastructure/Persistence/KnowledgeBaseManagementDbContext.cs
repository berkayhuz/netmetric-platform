// <copyright file="KnowledgeBaseManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.KnowledgeBaseManagement.Domain.Entities;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.KnowledgeBaseManagement.Infrastructure.Persistence;

public sealed class KnowledgeBaseManagementDbContext : DbContext, IKnowledgeBaseManagementDbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public KnowledgeBaseManagementDbContext(
        DbContextOptions<KnowledgeBaseManagementDbContext> options,
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

    public DbSet<KnowledgeBaseCategory> Categories => Set<KnowledgeBaseCategory>();
    public DbSet<KnowledgeBaseArticle> Articles => Set<KnowledgeBaseArticle>();
    public Guid? CurrentTenantId => _tenantProvider.TenantId;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KnowledgeBaseManagementDbContext).Assembly);

        modelBuilder.Entity<KnowledgeBaseCategory>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<KnowledgeBaseArticle>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);

        base.OnModelCreating(modelBuilder);
    }
}
