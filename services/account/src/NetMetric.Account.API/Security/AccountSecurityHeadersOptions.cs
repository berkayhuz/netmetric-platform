// <copyright file="AccountSecurityHeadersOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;

namespace NetMetric.Account.Api.Security;

public sealed class AccountSecurityHeadersOptions
{
    public const string SectionName = "Security:Headers";

    public string ContentSecurityPolicy { get; init; } = "default-src 'none'; frame-ancestors 'none'; frame-src 'none'; base-uri 'none'; object-src 'none'; form-action 'none';";
    public string ReferrerPolicy { get; init; } = "no-referrer";
    public string PermissionsPolicy { get; init; } = "camera=(), geolocation=(), microphone=()";
    public bool EnableHsts { get; init; } = true;
    public int HstsMaxAgeSeconds { get; init; } = 31536000;
}

public sealed class AccountSecurityHeadersOptionsValidation : IValidateOptions<AccountSecurityHeadersOptions>
{
    private static readonly string[] RequiredCspDirectives =
    [
        "default-src 'none'",
        "frame-ancestors 'none'",
        "frame-src 'none'",
        "base-uri 'none'",
        "object-src 'none'",
        "form-action 'none'"
    ];

    public ValidateOptionsResult Validate(string? name, AccountSecurityHeadersOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ContentSecurityPolicy))
        {
            failures.Add("Security:Headers:ContentSecurityPolicy is required.");
        }
        else
        {
            foreach (var directive in RequiredCspDirectives)
            {
                if (!options.ContentSecurityPolicy.Contains(directive, StringComparison.OrdinalIgnoreCase))
                {
                    failures.Add($"Security:Headers:ContentSecurityPolicy must include {directive}.");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(options.ReferrerPolicy))
        {
            failures.Add("Security:Headers:ReferrerPolicy is required.");
        }

        if (string.IsNullOrWhiteSpace(options.PermissionsPolicy))
        {
            failures.Add("Security:Headers:PermissionsPolicy is required.");
        }

        if (options.HstsMaxAgeSeconds is < 300 or > 31536000)
        {
            failures.Add("Security:Headers:HstsMaxAgeSeconds must be between 300 and 31536000.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
