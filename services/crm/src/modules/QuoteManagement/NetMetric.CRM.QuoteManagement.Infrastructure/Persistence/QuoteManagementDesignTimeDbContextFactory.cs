// <copyright file="QuoteManagementDesignTimeDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NetMetric.Tenancy;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Persistence;

public sealed class QuoteManagementDesignTimeDbContextFactory : IDesignTimeDbContextFactory<QuoteManagementDbContext>
{
    public QuoteManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__QuoteManagementConnection")
            ?? Environment.GetEnvironmentVariable("QUOTE_MANAGEMENT_CONNECTION_STRING")
            ?? "Server=localhost;Database=NetMetricCrmQuoteManagement;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<QuoteManagementDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(QuoteManagementDbContext).Assembly.FullName));

        return new QuoteManagementDbContext(optionsBuilder.Options, new DesignTimeTenantProvider());
    }

    private sealed class DesignTimeTenantProvider : ITenantProvider
    {
        public Guid? TenantId => null;
    }
}
