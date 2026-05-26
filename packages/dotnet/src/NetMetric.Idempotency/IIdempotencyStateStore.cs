// <copyright file="IIdempotencyStateStore.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Idempotency;

public interface IIdempotencyStateStore
{
    Task<IdempotencyState?> GetAsync(string key, CancellationToken cancellationToken);

    Task<bool> TryMarkInProgressAsync(string key, string requestHash, TimeSpan ttl, CancellationToken cancellationToken);

    Task MarkCompletedAsync(string key, string requestHash, string responseJson, TimeSpan ttl, CancellationToken cancellationToken);

    Task RemoveAsync(string key, CancellationToken cancellationToken);
}
