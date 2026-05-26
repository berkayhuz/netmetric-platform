// <copyright file="ArtificialIntelligenceDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ArtificialIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.ArtificialIntelligence.Domain.Entities;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.ArtificialIntelligence.Infrastructure.Persistence;

public sealed class ArtificialIntelligenceDbContext : AppDbContext, IArtificialIntelligenceDbContext
{
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public ArtificialIntelligenceDbContext(
        DbContextOptions<ArtificialIntelligenceDbContext> options,
        ITenantContext tenantContext,
        TenantIsolationSaveChangesInterceptor tenantInterceptor,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor) : base(options, tenantContext)
    {
        _tenantInterceptor = tenantInterceptor;
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
    }

    public DbSet<AiProviderConnection> ProviderConnections => Set<AiProviderConnection>();
    public DbSet<AiAutomationPolicy> AutomationPolicies => Set<AiAutomationPolicy>();
    public DbSet<AiInsightRun> InsightRuns => Set<AiInsightRun>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArtificialIntelligenceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
