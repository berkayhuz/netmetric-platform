// <copyright file="UserSessionStateValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NetMetric.Auth.Application.Abstractions;
using NetMetric.Auth.Application.Diagnostics;
using NetMetric.Auth.Application.Options;
using NetMetric.Clock;

namespace NetMetric.Auth.Infrastructure.Services;

public sealed class UserSessionStateValidator(
    IUserSessionRepository userSessionRepository,
    IAuthSessionService authSessionService,
    IDistributedCache distributedCache,
    IOptions<TokenValidationCacheOptions> options,
    IClock clock) : IUserSessionStateValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<bool> IsValidAsync(Guid tenantId, Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var value = options.Value;
        var cacheKey = BuildCacheKey(value.KeyPrefix, tenantId, sessionId);

        if (value.EnableCache)
        {
            var cachedPayload = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cachedPayload))
            {
                AuthMetrics.SessionStateCacheHit.Add(1);
                var cachedState = JsonSerializer.Deserialize<CachedSessionState>(cachedPayload, JsonOptions);
                if (cachedState is not null && cachedState.UserId == userId)
                {
                    return false;
                }
            }

            AuthMetrics.SessionStateCacheMiss.Add(1);
        }

        var session = await userSessionRepository.GetAsync(tenantId, sessionId, cancellationToken);
        var isValid = session is not null &&
                      session.UserId == userId &&
                      !session.IsRevoked &&
                      !authSessionService.IsExpired(session, clock.UtcDateTime);

        if (value.EnableCache && !isValid)
        {
            await distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(new CachedSessionState(userId), JsonOptions),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(value.NegativeAbsoluteExpirationSeconds)
                },
                cancellationToken);
        }

        return isValid;
    }

    public void Evict(Guid tenantId, Guid sessionId)
    {
        var value = options.Value;
        if (!value.EnableCache)
        {
            return;
        }

        distributedCache.Remove(BuildCacheKey(value.KeyPrefix, tenantId, sessionId));
    }

    private static string BuildCacheKey(string prefix, Guid tenantId, Guid sessionId) =>
        $"{prefix}:session:{tenantId:N}:{sessionId:N}";

    private sealed record CachedSessionState(Guid UserId);
}
