// <copyright file="INotificationDispatcher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Notification.Contracts.Notifications.Requests;
using NetMetric.Notification.Contracts.Notifications.Responses;

namespace NetMetric.Notification.Application.Abstractions;

public interface INotificationDispatcher
{
    Task<SendNotificationResponse> SendAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken = default);
}
