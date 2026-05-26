// <copyright file="AccountRateLimitingOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;

namespace NetMetric.Account.Api.RateLimiting;

public sealed class AccountRateLimitingOptions
{
    public const string SectionName = "Security:RateLimiting";

    public RateLimitRule Global { get; init; } = new(600, 60, 0);
    public RateLimitRule Critical { get; init; } = new(20, 60, 0);
}

public sealed record RateLimitRule(int PermitLimit, int WindowSeconds, int QueueLimit);

public sealed class AccountRateLimitingOptionsValidation : IValidateOptions<AccountRateLimitingOptions>
{
    public ValidateOptionsResult Validate(string? name, AccountRateLimitingOptions options)
    {
        var failures = new List<string>();

        ValidateRule(options.Global, "Security:RateLimiting:Global", failures);
        ValidateRule(options.Critical, "Security:RateLimiting:Critical", failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateRule(RateLimitRule rule, string prefix, ICollection<string> failures)
    {
        if (rule.PermitLimit <= 0)
        {
            failures.Add($"{prefix}:PermitLimit must be greater than 0.");
        }

        if (rule.WindowSeconds <= 0)
        {
            failures.Add($"{prefix}:WindowSeconds must be greater than 0.");
        }

        if (rule.QueueLimit < 0)
        {
            failures.Add($"{prefix}:QueueLimit must be 0 or greater.");
        }
    }
}
