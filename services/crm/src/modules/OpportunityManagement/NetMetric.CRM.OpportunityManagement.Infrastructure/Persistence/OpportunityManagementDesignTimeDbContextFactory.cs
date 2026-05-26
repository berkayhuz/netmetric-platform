// <copyright file="OpportunityManagementDesignTimeDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NetMetric.Tenancy;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence;

public sealed class OpportunityManagementDesignTimeDbContextFactory : IDesignTimeDbContextFactory<OpportunityManagementDbContext>
{
    public OpportunityManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__OpportunityManagementConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings:OpportunityManagementConnection")
            ?? "Server=localhost;Database=CRM.OpportunityManagementDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<OpportunityManagementDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(OpportunityManagementDbContext).Assembly.FullName));

        return new OpportunityManagementDbContext(optionsBuilder.Options, new DesignTimeTenantProvider());
    }

    private sealed class DesignTimeTenantProvider : ITenantProvider
    {
        public Guid? TenantId => null;
    }
}
