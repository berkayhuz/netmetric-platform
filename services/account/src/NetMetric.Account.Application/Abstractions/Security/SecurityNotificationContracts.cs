// <copyright file="SecurityNotificationContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Application.Abstractions.Security;

public sealed record SecurityNotificationRequest(
    Guid TenantId,
    Guid UserId,
    string NotificationType,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset OccurredAt);

public interface ISecurityNotificationPublisher
{
    Task PublishAsync(SecurityNotificationRequest request, CancellationToken cancellationToken = default);
}
