// <copyright file="AccountForwardedHeadersOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;
using NetMetric.AspNetCore.Security;

namespace NetMetric.Account.Api.Security;

public sealed class AccountForwardedHeadersOptions
{
    public const string SectionName = "Security:ForwardedHeaders";

    public int ForwardLimit { get; init; } = 2;
    public string[] KnownProxies { get; init; } = [];
    public string[] KnownNetworks { get; init; } = [];
}

public sealed class AccountForwardedHeadersOptionsValidation(IHostEnvironment environment) : IValidateOptions<AccountForwardedHeadersOptions>
{
    public ValidateOptionsResult Validate(string? name, AccountForwardedHeadersOptions options)
    {
        var failures = new List<string>();

        if (options.ForwardLimit <= 0)
        {
            failures.Add("Security:ForwardedHeaders:ForwardLimit must be greater than zero.");
        }

        foreach (var proxy in options.KnownProxies)
        {
            if (!System.Net.IPAddress.TryParse(proxy, out _))
            {
                failures.Add($"Security:ForwardedHeaders:KnownProxies contains invalid IP '{proxy}'.");
            }
        }

        foreach (var network in options.KnownNetworks)
        {
            if (!SecuritySupport.TryParseNetwork(network, out _))
            {
                failures.Add($"Security:ForwardedHeaders:KnownNetworks contains invalid CIDR '{network}'.");
            }
        }

        if (environment.IsProduction() && options.KnownProxies.Length == 0 && options.KnownNetworks.Length == 0)
        {
            failures.Add("Security:ForwardedHeaders must define at least one known proxy or known network in production.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
