// <copyright file="ProductCatalogDesignTimeDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.ProductCatalog.Infrastructure.Persistence;

public sealed class ProductCatalogDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProductCatalogDbContext>
{
    public ProductCatalogDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("src/Services/CRM/NetMetric.CRM.API/appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = Environment.GetEnvironmentVariable("NETMETRIC_PRODUCTCATALOG_MIGRATIONS_CONNECTION")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__ProductCatalogConnection")
            ?? configuration.GetConnectionString("ProductCatalogConnection")
            ?? "Server=localhost,14339;Database=CRM.ProductCatalogDb;User Id=sa;TrustServerCertificate=True;Encrypt=False;";

        var optionsBuilder = new DbContextOptionsBuilder<ProductCatalogDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ProductCatalogDbContext).Assembly.FullName));

        return new ProductCatalogDbContext(
            optionsBuilder.Options,
            new DesignTimeTenantContext(),
            new TenantIsolationSaveChangesInterceptor(),
            new AuditSaveChangesInterceptor(),
            new SoftDeleteSaveChangesInterceptor());
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
    }
}
