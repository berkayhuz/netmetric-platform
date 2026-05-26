// <copyright file="CustomerManagementDesignTimeDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NetMetric.Tenancy;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;

public sealed class CustomerManagementDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CustomerManagementDbContext>
{
    public CustomerManagementDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("src/Services/CRM/NetMetric.CRM.API/appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("CustomerManagementConnection")
            ?? "Server=localhost;Database=CRM.CustomerManagementDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<CustomerManagementDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(CustomerManagementDbContext).Assembly.FullName));

        return new CustomerManagementDbContext(optionsBuilder.Options, new DesignTimeTenantProvider());
    }

    private sealed class DesignTimeTenantProvider : ITenantProvider
    {
        public Guid? TenantId => null;
    }
}
