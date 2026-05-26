// <copyright file="ISmsProvider.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Notification.Infrastructure.Channels;

public interface ISmsProvider
{
    string Name { get; }

    Task<string?> SendAsync(
        string phoneNumber,
        string message,
        string? correlationId,
        CancellationToken cancellationToken);
}
