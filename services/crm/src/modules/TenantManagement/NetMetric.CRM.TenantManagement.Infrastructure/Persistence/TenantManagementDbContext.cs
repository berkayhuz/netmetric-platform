// <copyright file="TenantManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TenantManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TenantManagement.Domain.Entities;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Tenancy;

namespace NetMetric.CRM.TenantManagement.Infrastructure.Persistence;

public sealed class TenantManagementDbContext(
    DbContextOptions<TenantManagementDbContext> options,
    ITenantContext tenantContext)
    : AppDbContext(options, tenantContext), ITenantManagementDbContext
{
    public DbSet<TenantProfile> TenantProfiles => Set<TenantProfile>();
    public DbSet<TenantSettingEntry> TenantSettings => Set<TenantSettingEntry>();
    public DbSet<TenantFeatureFlag> TenantFeatureFlags => Set<TenantFeatureFlag>();
    public DbSet<TenantModuleToggle> TenantModuleToggles => Set<TenantModuleToggle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantProfile>(builder =>
        {
            builder.ToTable("TenantProfiles");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.TenantId).IsUnique();
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Locale).HasMaxLength(20).IsRequired();
            builder.Property(x => x.TimeZone).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<TenantSettingEntry>(builder =>
        {
            builder.ToTable("TenantSettings");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.TenantId, x.Section, x.Key }).IsUnique();
            builder.Property(x => x.Section).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Value).HasMaxLength(2000).IsRequired();
        });

        modelBuilder.Entity<TenantFeatureFlag>(builder =>
        {
            builder.ToTable("TenantFeatureFlags");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
            builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<TenantModuleToggle>(builder =>
        {
            builder.ToTable("TenantModuleToggles");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.TenantId, x.ModuleKey }).IsUnique();
            builder.Property(x => x.ModuleKey).HasMaxLength(100).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
