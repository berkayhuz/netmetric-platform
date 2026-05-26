// <copyright file="NotificationPreference.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Domain.Common;

namespace NetMetric.Account.Domain.Notifications;

public sealed class NotificationPreference
{
    private NotificationPreference()
    {
        Version = [];
    }

    private NotificationPreference(
        Guid id,
        TenantId tenantId,
        UserId userId,
        NotificationChannel channel,
        NotificationCategory category,
        bool isEnabled,
        DateTimeOffset utcNow)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        Channel = channel;
        Category = category;
        IsEnabled = isEnabled;
        CreatedAt = utcNow;
        UpdatedAt = utcNow;
        Version = [];
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public UserId UserId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationCategory Category { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] Version { get; private set; }

    public static NotificationPreference Create(
        TenantId tenantId,
        UserId userId,
        NotificationChannel channel,
        NotificationCategory category,
        bool isEnabled,
        DateTimeOffset utcNow)
        => new(Guid.NewGuid(), tenantId, userId, channel, category, isEnabled, utcNow);

    public void SetEnabled(bool isEnabled, DateTimeOffset utcNow)
    {
        if (Category == NotificationCategory.Security && !isEnabled)
        {
            throw new DomainValidationException("Security notifications cannot be disabled.");
        }

        IsEnabled = isEnabled;
        UpdatedAt = utcNow;
    }
}
