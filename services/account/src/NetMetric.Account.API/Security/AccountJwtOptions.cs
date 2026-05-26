// <copyright file="AccountJwtOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace NetMetric.Account.Api.Security;

public sealed class AccountJwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    public string Authority { get; init; } = string.Empty;
    public string? MetadataAddress { get; init; }
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ClockSkewSeconds { get; init; } = 60;
    public bool RequireHttpsMetadata { get; init; } = true;
}

public sealed class AccountJwtOptionsValidation(IHostEnvironment environment) : IValidateOptions<AccountJwtOptions>
{
    public ValidateOptionsResult Validate(string? name, AccountJwtOptions options)
    {
        var failures = new List<string>();

        ValidateProductionUrl(options.Authority, "Authentication:Jwt:Authority", failures);
        ValidateProductionUrl(options.Issuer, "Authentication:Jwt:Issuer", failures);

        if (!string.IsNullOrWhiteSpace(options.MetadataAddress))
        {
            ValidateProductionUrl(options.MetadataAddress, "Authentication:Jwt:MetadataAddress", failures);
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("Authentication:Jwt:Audience is required.");
        }
        else if (environment.IsProduction() && ContainsUnsafeMarker(options.Audience))
        {
            failures.Add("Authentication:Jwt:Audience contains a non-production marker.");
        }

        if (environment.IsProduction() && !options.RequireHttpsMetadata)
        {
            failures.Add("Authentication:Jwt:RequireHttpsMetadata must be true in production.");
        }

        if (options.ClockSkewSeconds is < 0 or > 300)
        {
            failures.Add("Authentication:Jwt:ClockSkewSeconds must be between 0 and 300 seconds.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private void ValidateProductionUrl(string value, string key, ICollection<string> failures)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && !IsDevelopmentHttpLoopback(uri)))
        {
            failures.Add($"{key} must be an absolute HTTPS URL.");
            return;
        }

        if (environment.IsProduction() &&
            (uri.IsLoopback || ContainsUnsafeMarker(uri.Host)))
        {
            failures.Add($"{key} must be a production HTTPS URL.");
        }
    }

    private bool IsDevelopmentHttpLoopback(Uri uri) =>
        environment.IsDevelopment() &&
        uri.Scheme == Uri.UriSchemeHttp &&
        uri.IsLoopback;

    private static bool ContainsUnsafeMarker(string value) =>
        value.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("REPLACE", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("LOCAL", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("DEV", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("TEST", StringComparison.OrdinalIgnoreCase);
}
