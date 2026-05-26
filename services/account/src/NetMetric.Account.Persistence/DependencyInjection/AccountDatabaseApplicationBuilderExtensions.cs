// <copyright file="AccountDatabaseApplicationBuilderExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.Account.Persistence.Options;

namespace NetMetric.Account.Persistence.DependencyInjection;

public static class AccountDatabaseApplicationBuilderExtensions
{
    public static async Task<IHost> ApplyAccountDatabaseMigrationsAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        using var scope = host.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<AccountDatabaseOptions>>().Value;

        if (!options.ApplyMigrationsOnStartup)
        {
            return host;
        }

        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException("Account database migrations cannot run automatically outside Development startup. Run the migration bundle or deployment migration job instead.");
        }

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("NetMetric.Account.Persistence.Migrations");
        var dbContext = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
        logger.LogInformation("Applying Account database migrations during {EnvironmentName} startup.", environment.EnvironmentName);
        await dbContext.Database.MigrateAsync(cancellationToken);

        return host;
    }
}
