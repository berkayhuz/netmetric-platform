// <copyright file="AccountDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NetMetric.Account.Persistence.DesignTime;

public sealed class AccountDbContextFactory : IDesignTimeDbContextFactory<AccountDbContext>
{
    public AccountDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("NETMETRIC_ACCOUNT_MIGRATIONS_CONNECTION")
            ?? "Server=localhost;Database=CRM.AccountDb;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<AccountDbContext>()
            .UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(AccountDbContext).Assembly.FullName))
            .Options;

        return new AccountDbContext(options);
    }
}
