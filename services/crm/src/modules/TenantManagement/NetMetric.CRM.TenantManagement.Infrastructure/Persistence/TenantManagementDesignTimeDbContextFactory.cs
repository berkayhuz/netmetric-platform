// <copyright file="TenantManagementDesignTimeDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NetMetric.Tenancy;

namespace NetMetric.CRM.TenantManagement.Infrastructure.Persistence;

public sealed class TenantManagementDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TenantManagementDbContext>
{
    public TenantManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TenantManagementConnection")
            ?? Environment.GetEnvironmentVariable("TENANT_MANAGEMENT_CONNECTION_STRING")
            ?? "Server=localhost;Database=NetMetricCrmTenantManagement;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<TenantManagementDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(TenantManagementDbContext).Assembly.FullName));

        return new TenantManagementDbContext(optionsBuilder.Options, new DesignTimeTenantContext());
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
    }
}
