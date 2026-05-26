// <copyright file="PipelineManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Domain.Entities;
using NetMetric.CRM.PipelineManagement.Infrastructure.Outbox;
using NetMetric.CRM.Sales;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Tenancy;

namespace NetMetric.CRM.PipelineManagement.Infrastructure.Persistence;

public sealed class PipelineManagementDbContext(
    DbContextOptions<PipelineManagementDbContext> options,
    ITenantContext tenantContext)
    : AppDbContext(options, tenantContext), IPipelineManagementDbContext
{
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<LostReason> LostReasons => Set<LostReason>();
    public DbSet<OpportunityStageHistory> OpportunityStageHistories => Set<OpportunityStageHistory>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Pipeline> Pipelines => Set<Pipeline>();
    public DbSet<PipelineStage> PipelineStages => Set<PipelineStage>();
    public DbSet<StageRequiredField> StageRequiredFields => Set<StageRequiredField>();
    public DbSet<StageExitCriteria> StageExitCriterias => Set<StageExitCriteria>();
    public DbSet<ForecastCategoryMapping> ForecastCategoryMappings => Set<ForecastCategoryMapping>();
    public DbSet<PipelineTemplate> PipelineTemplates => Set<PipelineTemplate>();
    public DbSet<PipelineSnapshot> PipelineSnapshots => Set<PipelineSnapshot>();
    public DbSet<PipelineHealthRule> PipelineHealthRules => Set<PipelineHealthRule>();
    public DbSet<PipelineAutomationTrigger> PipelineAutomationTriggers => Set<PipelineAutomationTrigger>();
    public DbSet<PipelineManagementOutboxMessage> OutboxMessages => Set<PipelineManagementOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PipelineManagementDbContext).Assembly);
        modelBuilder.ApplyDefaultDecimalPrecision();

        modelBuilder.Entity<Pipeline>(builder =>
        {
            builder.ToTable("Pipelines");
            builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1024);
            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<PipelineStage>(builder =>
        {
            builder.ToTable("PipelineStages");
            builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1024);
            builder.Property(x => x.Probability).HasPrecision(5, 2);
            builder.HasIndex(x => new { x.TenantId, x.PipelineId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<StageRequiredField>(builder =>
        {
            builder.ToTable("StageRequiredFields");
            builder.Property(x => x.FieldName).HasMaxLength(128).IsRequired();
            builder.Property(x => x.DisplayName).HasMaxLength(128);
            builder.Property(x => x.ValidationRule).HasMaxLength(512);
        });

        modelBuilder.Entity<StageExitCriteria>(builder =>
        {
            builder.ToTable("StageExitCriterias");
            builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1024);
        });

        modelBuilder.Entity<ForecastCategoryMapping>(builder =>
        {
            builder.ToTable("ForecastCategoryMappings");
            builder.Property(x => x.ExternalStageName).HasMaxLength(128).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.ExternalStageName }).IsUnique();
        });

        modelBuilder.Entity<PipelineTemplate>(builder =>
        {
            builder.ToTable("PipelineTemplates");
            builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Industry).HasMaxLength(128);
        });

        modelBuilder.Entity<PipelineSnapshot>(builder =>
        {
            builder.ToTable("PipelineSnapshots");
            builder.Property(x => x.TotalValue).HasPrecision(18, 2);
            builder.Property(x => x.WeightedValue).HasPrecision(18, 2);
            builder.HasIndex(x => new { x.TenantId, x.PipelineId, x.SnapshotDate });
        });

        modelBuilder.Entity<PipelineHealthRule>(builder =>
        {
            builder.ToTable("PipelineHealthRules");
            builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
            builder.Property(x => x.RuleType).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<PipelineAutomationTrigger>(builder =>
        {
            builder.ToTable("PipelineAutomationTriggers");
            builder.Property(x => x.ActionType).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<LostReason>(builder =>
        {
            builder.ToTable("LostReasons");
            builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1024);
            builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<OpportunityStageHistory>(builder =>
        {
            builder.ToTable("OpportunityStageHistories");
            builder.Property(x => x.Note).HasMaxLength(1024);
            builder.HasIndex(x => new { x.TenantId, x.OpportunityId, x.ChangedAt });
        });

        modelBuilder.Entity<Lead>(builder =>
        {
            builder.ToTable("Leads");
            builder.Property(x => x.LeadCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.FullName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.EstimatedBudget).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Opportunity>(builder =>
        {
            builder.ToTable("Opportunities");
            builder.Property(x => x.OpportunityCode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
            builder.Property(x => x.LostNote).HasMaxLength(1024);
            builder.Property(x => x.EstimatedAmount).HasPrecision(18, 2);
            builder.Property(x => x.ExpectedRevenue).HasPrecision(18, 2);
            builder.Property(x => x.Probability).HasPrecision(5, 2);
        });

        modelBuilder.Entity<Customer>(builder => builder.ToTable("Customers"));
        modelBuilder.Entity<Company>(builder =>
        {
            builder.ToTable("Companies");
            builder.Property(x => x.AnnualRevenue).HasPrecision(18, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}
