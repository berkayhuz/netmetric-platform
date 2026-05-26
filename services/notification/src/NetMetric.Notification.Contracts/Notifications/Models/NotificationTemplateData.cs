// <copyright file="NotificationTemplateData.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Notification.Contracts.Notifications.Models;

public sealed record NotificationTemplateData(
    string? TemplateKey,
    IReadOnlyDictionary<string, string> Values);
