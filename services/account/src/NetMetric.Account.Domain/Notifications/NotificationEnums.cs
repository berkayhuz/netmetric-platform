// <copyright file="NotificationEnums.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Domain.Notifications;

public enum NotificationChannel
{
    Email = 1,
    Sms = 2,
    InApp = 3,
    Push = 4
}

public enum NotificationCategory
{
    Security = 1,
    System = 2,
    Crm = 3,
    Marketing = 4
}
