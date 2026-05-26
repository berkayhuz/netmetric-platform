// <copyright file="ITenantManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TenantManagement.Domain.Entities;

namespace NetMetric.CRM.TenantManagement.Application.Abstractions.Persistence;

public interface ITenantManagementDbContext
{
    DbSet<TenantProfile> TenantProfiles { get; }
    DbSet<TenantSettingEntry> TenantSettings { get; }
    DbSet<TenantFeatureFlag> TenantFeatureFlags { get; }
    DbSet<TenantModuleToggle> TenantModuleToggles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
