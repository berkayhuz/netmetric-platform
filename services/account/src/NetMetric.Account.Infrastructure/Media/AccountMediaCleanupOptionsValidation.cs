// <copyright file="AccountMediaCleanupOptionsValidation.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;

namespace NetMetric.Account.Infrastructure.Media;

public sealed class AccountMediaCleanupOptionsValidation : IValidateOptions<AccountMediaCleanupOptions>
{
    public ValidateOptionsResult Validate(string? name, AccountMediaCleanupOptions options)
    {
        var failures = new List<string>();
        if (options.IntervalSeconds <= 0)
        {
            failures.Add("Account:MediaCleanup:IntervalSeconds must be greater than zero.");
        }

        if (options.GracePeriodMinutes < 0)
        {
            failures.Add("Account:MediaCleanup:GracePeriodMinutes cannot be negative.");
        }

        if (options.BatchSize <= 0 || options.BatchSize > 1000)
        {
            failures.Add("Account:MediaCleanup:BatchSize must be between 1 and 1000.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
