// <copyright file="DocumentManagementDesignTimeDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.DocumentManagement.Infrastructure.Persistence;

public sealed class DocumentManagementDesignTimeDbContextFactory : IDesignTimeDbContextFactory<DocumentManagementDbContext>
{
    public DocumentManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DocumentManagementConnection")
            ?? Environment.GetEnvironmentVariable("DOCUMENT_MANAGEMENT_CONNECTION_STRING")
            ?? "Server=localhost;Database=NetMetricCrmDocumentManagement;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<DocumentManagementDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sql => sql.MigrationsAssembly(typeof(DocumentManagementDbContext).Assembly.FullName));

        return new DocumentManagementDbContext(
            optionsBuilder.Options,
            new DesignTimeTenantContext(),
            new AuditSaveChangesInterceptor(),
            new SoftDeleteSaveChangesInterceptor(),
            new TenantIsolationSaveChangesInterceptor());
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid? TenantId => null;
    }
}
