// <copyright file="INotificationTemplateRenderer.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Notification.Contracts.Notifications.Requests;

namespace NetMetric.Notification.Application.Abstractions;

public interface INotificationTemplateRenderer
{
    SendNotificationRequest Render(SendNotificationRequest request);
}
