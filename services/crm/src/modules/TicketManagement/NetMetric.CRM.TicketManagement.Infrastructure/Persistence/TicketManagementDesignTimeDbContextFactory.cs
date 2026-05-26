// <copyright file="TicketManagementDesignTimeDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.TicketManagement.Infrastructure.Persistence;

public sealed class TicketManagementDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TicketManagementDbContext>
{
    public TicketManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TicketManagementConnection")
            ?? Environment.GetEnvironmentVariable("TICKET_MANAGEMENT_CONNECTION_STRING")
            ?? "Server=localhost;Database=NetMetricCrmTicketManagement;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<TicketManagementDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(TicketManagementDbContext).Assembly.FullName));

        var tenantProvider = new DesignTimeTenantProvider();
        return new TicketManagementDbContext(
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
