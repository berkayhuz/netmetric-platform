// <copyright file="GatewayHealthChecks.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NetMetric.AspNetCore.TrustedGateway.Options;
using Yarp.ReverseProxy.Configuration;

namespace NetMetric.ApiGateway.Health;

public sealed class GatewaySigningConfigurationHealthCheck(IOptions<TrustedGatewayOptions> options) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var value = options.Value;
        var currentKey = value.Keys.FirstOrDefault(
            x => x.Enabled &&
                 x.SignRequests &&
                 string.Equals(x.KeyId, value.CurrentKeyId, StringComparison.Ordinal));

        return Task.FromResult(
            currentKey is null || string.IsNullOrWhiteSpace(currentKey.Secret)
                ? HealthCheckResult.Unhealthy("Trusted gateway signing key is unavailable.")
                : HealthCheckResult.Healthy("Trusted gateway signing key is available."));
    }
}

public sealed class GatewayReverseProxyConfigurationHealthCheck(IProxyConfigProvider proxyConfigProvider) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var config = proxyConfigProvider.GetConfig();
        if (config.Routes.Count == 0 || config.Clusters.Count == 0)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Reverse proxy routes or clusters are not configured."));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Reverse proxy configuration loaded."));
    }
}
