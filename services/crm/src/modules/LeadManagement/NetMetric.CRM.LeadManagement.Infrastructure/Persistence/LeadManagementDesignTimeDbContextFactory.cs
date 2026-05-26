// <copyright file="LeadManagementDesignTimeDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NetMetric.Tenancy;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Persistence;

public sealed class LeadManagementDesignTimeDbContextFactory : IDesignTimeDbContextFactory<LeadManagementDbContext>
{
    public LeadManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__LeadManagementConnection")
            ?? Environment.GetEnvironmentVariable("LEAD_MANAGEMENT_CONNECTION_STRING")
            ?? "Server=localhost;Database=NetMetricCrmLeadManagement;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<LeadManagementDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(LeadManagementDbContext).Assembly.FullName));

        return new LeadManagementDbContext(optionsBuilder.Options, new DesignTimeTenantProvider());
    }

    private sealed class DesignTimeTenantProvider : ITenantProvider
    {
        public Guid? TenantId => null;
    }
}
