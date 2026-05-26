// <copyright file="IntegrationHubDesignTimeDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Persistence;

public sealed class IntegrationHubDesignTimeDbContextFactory : IDesignTimeDbContextFactory<IntegrationHubDbContext>
{
    public IntegrationHubDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__IntegrationHubConnection")
            ?? Environment.GetEnvironmentVariable("INTEGRATIONHUB_CONNECTION_STRING")
            ?? "Server=localhost;Database=CRM.IntegrationHubDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<IntegrationHubDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(IntegrationHubDbContext).Assembly.FullName));

        var dataProtectionProvider = new EphemeralDataProtectionProvider();
        return new IntegrationHubDbContext(optionsBuilder.Options, new DesignTimeTenantContext(), dataProtectionProvider);
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
    }
}
