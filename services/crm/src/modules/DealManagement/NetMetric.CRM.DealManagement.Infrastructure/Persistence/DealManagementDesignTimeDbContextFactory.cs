// <copyright file="DealManagementDesignTimeDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.DealManagement.Infrastructure.Persistence;

public sealed class DealManagementDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DealManagementDbContext>
{
    public DealManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DealManagementConnection")
            ?? Environment.GetEnvironmentVariable("DEAL_MANAGEMENT_CONNECTION_STRING")
            ?? "Server=localhost;Database=NetMetricCrmDealManagement;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<DealManagementDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(DealManagementDbContext).Assembly.FullName));

        var tenantProvider = new DesignTimeTenantProvider();
        return new DealManagementDbContext(
            optionsBuilder.Options,
            tenantProvider,
            new TenantIsolationSaveChangesInterceptor(tenantProvider: tenantProvider),
            new AuditSaveChangesInterceptor(),
            new SoftDeleteSaveChangesInterceptor());
    }

    private sealed class DesignTimeTenantProvider : ITenantProvider
    {
        public Guid? TenantId => null;
    }
}
