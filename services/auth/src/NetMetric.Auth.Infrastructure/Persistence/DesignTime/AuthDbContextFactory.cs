// <copyright file="AuthDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NetMetric.Auth.Infrastructure.Persistence.DesignTime;

public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("NETMETRIC_AUTH_MIGRATIONS_CONNECTION")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__IdentityConnection")
            ?? "Server=localhost;Database=CRM.AuthDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName))
            .Options;

        return new AuthDbContext(options);
    }
}
