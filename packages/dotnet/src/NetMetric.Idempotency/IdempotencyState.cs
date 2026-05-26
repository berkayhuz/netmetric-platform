// <copyright file="IdempotencyState.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Idempotency;

public sealed record IdempotencyState(
    IdempotencyStatus Status,
    string RequestHash,
    string ResponseJson)
{
    public static IdempotencyState InProgress(string requestHash)
        => new(IdempotencyStatus.InProgress, requestHash, string.Empty);

    public static IdempotencyState Completed(string requestHash, string responseJson)
        => new(IdempotencyStatus.Completed, requestHash, responseJson);
}
