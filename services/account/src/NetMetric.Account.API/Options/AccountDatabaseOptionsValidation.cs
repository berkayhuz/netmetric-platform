// <copyright file="AccountDatabaseOptionsValidation.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;
using NetMetric.Account.Persistence.Options;

namespace NetMetric.Account.Api.Options;

public sealed class AccountDatabaseOptionsValidation(
    IHostEnvironment environment,
    IConfiguration configuration) : IValidateOptions<AccountDatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, AccountDatabaseOptions options)
    {
        var failures = new List<string>();
        var connectionString = configuration.GetConnectionString("AccountDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            failures.Add("ConnectionStrings:AccountDb is required. Supply it with the ConnectionStrings__AccountDb environment variable or user-secrets.");
        }

        if (environment.IsProduction())
        {
            if (options.ApplyMigrationsOnStartup)
            {
                failures.Add("Database:ApplyMigrationsOnStartup must be false in production.");
            }

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                if (connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                    connectionString.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                    connectionString.Contains("(local)", StringComparison.OrdinalIgnoreCase))
                {
                    failures.Add("ConnectionStrings:AccountDb cannot point to localhost in production.");
                }

                if (connectionString.Contains("TrustServerCertificate=True", StringComparison.OrdinalIgnoreCase))
                {
                    failures.Add("ConnectionStrings:AccountDb cannot use TrustServerCertificate=True in production.");
                }
            }
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
