// <copyright file="FinanceOperationsDesignTimeDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.FinanceOperations.Infrastructure.Persistence;

public sealed class FinanceOperationsDesignTimeDbContextFactory : IDesignTimeDbContextFactory<FinanceOperationsDbContext>
{
    public FinanceOperationsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__FinanceOperationsConnection")
            ?? Environment.GetEnvironmentVariable("FINANCE_OPERATIONS_CONNECTION_STRING")
            ?? "Server=localhost;Database=NetMetricCrmFinanceOperations;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<FinanceOperationsDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(FinanceOperationsDbContext).Assembly.FullName));

        var tenantContext = new DesignTimeTenantContext();
        return new FinanceOperationsDbContext(
            optionsBuilder.Options,
            tenantContext,
            new TenantIsolationSaveChangesInterceptor(tenantContext),
            new AuditSaveChangesInterceptor(),
            new SoftDeleteSaveChangesInterceptor());
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
    }
}
