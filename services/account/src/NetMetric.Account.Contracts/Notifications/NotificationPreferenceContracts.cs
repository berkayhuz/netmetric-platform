// <copyright file="NotificationPreferenceContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Notifications;

public sealed record NotificationPreferenceItemResponse(
    Guid Id,
    string Channel,
    string Category,
    bool IsEnabled,
    string Version);

public sealed record NotificationPreferencesResponse(IReadOnlyCollection<NotificationPreferenceItemResponse> Items);

public sealed record UpdateNotificationPreferenceItemRequest(
    string Channel,
    string Category,
    bool IsEnabled);

public sealed record UpdateNotificationPreferencesRequest(
    IReadOnlyCollection<UpdateNotificationPreferenceItemRequest> Items);
