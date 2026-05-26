// <copyright file="AccountPersistenceServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Persistence.Options;
using NetMetric.Account.Persistence.Repositories;

namespace NetMetric.Account.Persistence.DependencyInjection;

public static class AccountPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddAccountPersistence(
        this IServiceCollection services,
        string connectionString,
        Action<AccountDatabaseOptions>? configureDatabase = null)
    {
        var databaseOptions = new AccountDatabaseOptions();
        configureDatabase?.Invoke(databaseOptions);

        services.AddDbContext<AccountDbContext>((provider, options) =>
        {
            var configuredOptions = provider.GetService<IOptions<AccountDatabaseOptions>>()?.Value ?? databaseOptions;
            options.UseSqlServer(
                connectionString,
                sql =>
                {
                    sql.MigrationsAssembly(typeof(AccountDbContext).Assembly.FullName);
                    sql.CommandTimeout(configuredOptions.CommandTimeoutSeconds);
                    sql.EnableRetryOnFailure(
                        configuredOptions.MaxRetryCount,
                        TimeSpan.FromSeconds(configuredOptions.MaxRetryDelaySeconds),
                        null);
                });
        });

        services.AddScoped<IAccountDbContext>(provider => provider.GetRequiredService<AccountDbContext>());
        services.AddScoped<IRepository<IAccountDbContext, Domain.Profiles.UserProfile>>(provider =>
            new EfRepository<IAccountDbContext, Domain.Profiles.UserProfile>(provider.GetRequiredService<AccountDbContext>()));
        services.AddScoped<IRepository<IAccountDbContext, Domain.Profiles.AccountMediaAsset>>(provider =>
            new EfRepository<IAccountDbContext, Domain.Profiles.AccountMediaAsset>(provider.GetRequiredService<AccountDbContext>()));
        services.AddScoped<IRepository<IAccountDbContext, Domain.Preferences.UserPreference>>(provider =>
            new EfRepository<IAccountDbContext, Domain.Preferences.UserPreference>(provider.GetRequiredService<AccountDbContext>()));
        services.AddScoped<IRepository<IAccountDbContext, Domain.Notifications.NotificationPreference>>(provider =>
            new EfRepository<IAccountDbContext, Domain.Notifications.NotificationPreference>(provider.GetRequiredService<AccountDbContext>()));
        services.AddScoped<IRepository<IAccountDbContext, Domain.Notifications.UserNotificationState>>(provider =>
            new EfRepository<IAccountDbContext, Domain.Notifications.UserNotificationState>(provider.GetRequiredService<AccountDbContext>()));
        services.AddScoped<IRepository<IAccountDbContext, Domain.Sessions.UserSession>>(provider =>
            new EfRepository<IAccountDbContext, Domain.Sessions.UserSession>(provider.GetRequiredService<AccountDbContext>()));
        services.AddScoped<IRepository<IAccountDbContext, Domain.Audit.AccountAuditEntry>>(provider =>
            new EfRepository<IAccountDbContext, Domain.Audit.AccountAuditEntry>(provider.GetRequiredService<AccountDbContext>()));
        services.AddScoped<IRepository<IAccountDbContext, Domain.Devices.TrustedDevice>>(provider =>
            new EfRepository<IAccountDbContext, Domain.Devices.TrustedDevice>(provider.GetRequiredService<AccountDbContext>()));
        services.AddScoped<IRepository<IAccountDbContext, Domain.Consents.UserConsent>>(provider =>
            new EfRepository<IAccountDbContext, Domain.Consents.UserConsent>(provider.GetRequiredService<AccountDbContext>()));
        services.AddScoped<IRepository<IAccountDbContext, Domain.Outbox.AccountOutboxMessage>>(provider =>
            new EfRepository<IAccountDbContext, Domain.Outbox.AccountOutboxMessage>(provider.GetRequiredService<AccountDbContext>()));
        services.AddScoped<IConcurrencyTokenWriter, EfConcurrencyTokenWriter>();

        return services;
    }
}
