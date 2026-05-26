// <copyright file="AnalyticsReportingDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Persistence;
using NetMetric.CRM.AnalyticsReporting.Domain.Entities;
using NetMetric.Entities;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Tenancy;

namespace NetMetric.CRM.AnalyticsReporting.Infrastructure.Persistence;

public sealed class AnalyticsReportingDbContext(
    DbContextOptions<AnalyticsReportingDbContext> options,
    ITenantContext tenantContext)
    : AppDbContext(options, tenantContext), IAnalyticsReportingDbContext
{
    public DbSet<DashboardWidget> DashboardWidgets => Set<DashboardWidget>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<AnalyticsTenantSnapshot> TenantSnapshots => Set<AnalyticsTenantSnapshot>();
    public DbSet<AnalyticsSalesFunnelSnapshot> SalesFunnelSnapshots => Set<AnalyticsSalesFunnelSnapshot>();
    public DbSet<AnalyticsCampaignRoiSnapshot> CampaignRoiSnapshots => Set<AnalyticsCampaignRoiSnapshot>();
    public DbSet<AnalyticsRevenueAgingSnapshot> RevenueAgingSnapshots => Set<AnalyticsRevenueAgingSnapshot>();
    public DbSet<AnalyticsSupportKpiSnapshot> SupportKpiSnapshots => Set<AnalyticsSupportKpiSnapshot>();
    public DbSet<AnalyticsUserProductivitySnapshot> UserProductivitySnapshots => Set<AnalyticsUserProductivitySnapshot>();
    public DbSet<AnalyticsProjectionRun> ProjectionRuns => Set<AnalyticsProjectionRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var usesSqlServerRowVersion = Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true;

        modelBuilder.Entity<DashboardWidget>(builder =>
        {
            builder.ToTable("DashboardWidgets");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.WidgetKey).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
            builder.Property(x => x.RoleName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.DataSource).HasMaxLength(100).IsRequired();
            ConfigureRowVersion(builder);
            builder.HasIndex(x => new { x.TenantId, x.RoleName, x.DisplayOrder });
        });

        modelBuilder.Entity<ReportDefinition>(builder =>
        {
            builder.ToTable("ReportDefinitions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
            builder.Property(x => x.QueryKey).HasMaxLength(120).IsRequired();
            ConfigureRowVersion(builder);
            builder.HasIndex(x => new { x.TenantId, x.QueryKey }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<AnalyticsTenantSnapshot>(builder =>
        {
            builder.ToTable("AnalyticsTenantSnapshots");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TenantName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Revenue).HasPrecision(18, 2);
            ConfigureRowVersion(builder);
            builder.HasIndex(x => new { x.TenantId, x.SnapshotAtUtc });
        });

        modelBuilder.Entity<AnalyticsSalesFunnelSnapshot>(builder =>
        {
            builder.ToTable("AnalyticsSalesFunnelSnapshots");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PipelineValue).HasPrecision(18, 2);
            ConfigureRowVersion(builder);
            builder.HasIndex(x => new { x.TenantId, x.SnapshotAtUtc });
        });

        modelBuilder.Entity<AnalyticsCampaignRoiSnapshot>(builder =>
        {
            builder.ToTable("AnalyticsCampaignRoiSnapshots");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CampaignName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Spend).HasPrecision(18, 2);
            builder.Property(x => x.Revenue).HasPrecision(18, 2);
            builder.Property(x => x.RoiPercentage).HasPrecision(9, 2);
            ConfigureRowVersion(builder);
            builder.HasIndex(x => new { x.TenantId, x.SnapshotAtUtc });
            builder.HasIndex(x => new { x.TenantId, x.CampaignName, x.SnapshotAtUtc });
        });

        modelBuilder.Entity<AnalyticsRevenueAgingSnapshot>(builder =>
        {
            builder.ToTable("AnalyticsRevenueAgingSnapshots");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CurrentAmount).HasPrecision(18, 2);
            builder.Property(x => x.Days30).HasPrecision(18, 2);
            builder.Property(x => x.Days60).HasPrecision(18, 2);
            builder.Property(x => x.Days90Plus).HasPrecision(18, 2);
            ConfigureRowVersion(builder);
            builder.HasIndex(x => new { x.TenantId, x.SnapshotAtUtc });
        });

        modelBuilder.Entity<AnalyticsSupportKpiSnapshot>(builder =>
        {
            builder.ToTable("AnalyticsSupportKpiSnapshots");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FirstResponseHours).HasPrecision(9, 2);
            builder.Property(x => x.ResolutionHours).HasPrecision(9, 2);
            ConfigureRowVersion(builder);
            builder.HasIndex(x => new { x.TenantId, x.SnapshotAtUtc });
        });

        modelBuilder.Entity<AnalyticsUserProductivitySnapshot>(builder =>
        {
            builder.ToTable("AnalyticsUserProductivitySnapshots");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserName).HasMaxLength(220).IsRequired();
            ConfigureRowVersion(builder);
            builder.HasIndex(x => new { x.TenantId, x.SnapshotAtUtc });
            builder.HasIndex(x => new { x.TenantId, x.UserId, x.SnapshotAtUtc });
        });

        modelBuilder.Entity<AnalyticsProjectionRun>(builder =>
        {
            builder.ToTable("AnalyticsProjectionRuns");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
            builder.HasIndex(x => x.StartedAtUtc);
            builder.HasIndex(x => x.Status);
        });

        base.OnModelCreating(modelBuilder);

        void ConfigureRowVersion<TEntity>(EntityTypeBuilder<TEntity> builder)
            where TEntity : EntityBase
        {
            var property = builder.Property(x => x.RowVersion);
            if (usesSqlServerRowVersion)
            {
                property.IsRowVersion();
                return;
            }

            property.IsConcurrencyToken().ValueGeneratedNever();
        }
    }
}
