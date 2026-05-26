// <copyright file="IdentityResilienceHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetMetric.Account.Infrastructure.Identity;

public sealed class IdentityResilienceHandler(
    IOptions<IdentityServiceOptions> options,
    ILogger<IdentityResilienceHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var maxAttempts = request.Method == HttpMethod.Get
            ? Math.Max(1, options.Value.GetRetryCount + 1)
            : 1;

        for (var attempt = 1; ; attempt++)
        {
            using var attemptRequest = CloneGetRequest(request);
            try
            {
                var response = await base.SendAsync(attemptRequest, cancellationToken);
                if (!ShouldRetry(response.StatusCode) || attempt >= maxAttempts)
                {
                    return response;
                }

                response.Dispose();
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                logger.LogWarning("Transient identity service transport failure on attempt {Attempt}.", attempt);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(options.Value.GetRetryDelayMilliseconds * attempt), cancellationToken);
        }
    }

    private static bool ShouldRetry(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout;

    private static HttpRequestMessage CloneGetRequest(HttpRequestMessage source)
    {
        var clone = new HttpRequestMessage(source.Method, source.RequestUri)
        {
            Version = source.Version,
            VersionPolicy = source.VersionPolicy
        };

        foreach (var header in source.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in source.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        return clone;
    }
}
