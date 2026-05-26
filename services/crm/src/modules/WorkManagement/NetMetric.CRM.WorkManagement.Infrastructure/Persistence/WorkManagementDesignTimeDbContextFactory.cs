// <copyright file="WorkManagementDesignTimeDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.WorkManagement.Infrastructure.Persistence;

public sealed class WorkManagementDesignTimeDbContextFactory : IDesignTimeDbContextFactory<WorkManagementDbContext>
{
    public WorkManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__WorkManagementConnection")
            ?? Environment.GetEnvironmentVariable("WORK_MANAGEMENT_CONNECTION_STRING")
            ?? "Server=localhost;Database=NetMetricCrmWorkManagement;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<WorkManagementDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(WorkManagementDbContext).Assembly.FullName));

        var tenantProvider = new DesignTimeTenantProvider();
        return new WorkManagementDbContext(
            optionsBuilder.Options,
            tenantProvider,
            new TenantIsolationSaveChangesInterceptor(tenantProvider),
            new AuditSaveChangesInterceptor(),
            new SoftDeleteSaveChangesInterceptor());
    }

    private sealed class DesignTimeTenantProvider : ITenantProvider, ITenantContext
    {
        public Guid? TenantId => null;
    }
}
