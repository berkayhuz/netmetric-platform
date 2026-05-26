// <copyright file="IPushProvider.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Notification.Infrastructure.Channels;

public interface IPushProvider
{
    string Name { get; }

    Task<string?> SendAsync(
        string pushToken,
        string title,
        string body,
        string? correlationId,
        CancellationToken cancellationToken);
}
