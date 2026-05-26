// <copyright file="TagManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TagManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TagManagement.Domain.Entities.ClassificationSchemes;
using NetMetric.CRM.TagManagement.Domain.Entities.SmartLabelRules;
using NetMetric.CRM.TagManagement.Domain.Entities.TagAssignments;
using NetMetric.CRM.TagManagement.Domain.Entities.TagDefinitions;
using NetMetric.CRM.TagManagement.Domain.Entities.TagGroups;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.TagManagement.Infrastructure.Persistence;

public sealed class TagManagementDbContext : AppDbContext, ITagManagementDbContext
{
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;

    public TagManagementDbContext(
        DbContextOptions<TagManagementDbContext> options,
        ITenantContext tenantContext,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor,
        TenantIsolationSaveChangesInterceptor tenantInterceptor) : base(options, tenantContext)
    {
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
        _tenantInterceptor = tenantInterceptor;
    }

    public DbSet<TagDefinition> TagDefinitions => Set<TagDefinition>();
    public DbSet<TagGroup> TagGroups => Set<TagGroup>();
    public DbSet<TagAssignment> TagAssignments => Set<TagAssignment>();
    public DbSet<SmartLabelRule> SmartLabelRules => Set<SmartLabelRule>();
    public DbSet<ClassificationScheme> ClassificationSchemes => Set<ClassificationScheme>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TagManagementDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
