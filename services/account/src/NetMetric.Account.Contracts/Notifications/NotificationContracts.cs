// <copyright file="NotificationContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Notifications;

public sealed record AccountNotificationResponse(
    Guid Id,
    string Title,
    string Description,
    string Category,
    string Severity,
    DateTimeOffset OccurredAt,
    bool IsRead);

public sealed record AccountNotificationsResponse(
    IReadOnlyCollection<AccountNotificationResponse> Items,
    int TotalCount,
    int UnreadCount,
    int ReadCount);
