// <copyright file="UserNotificationState.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Domain.Common;

namespace NetMetric.Account.Domain.Notifications;

public sealed class UserNotificationState
{
    private UserNotificationState()
    {
        Version = [];
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public UserId UserId { get; private set; }
    public Guid NotificationId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] Version { get; private set; }

    public static UserNotificationState Create(TenantId tenantId, UserId userId, Guid notificationId, DateTimeOffset utcNow)
        => new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            NotificationId = notificationId,
            IsRead = false,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            Version = []
        };

    public void MarkRead(DateTimeOffset utcNow)
    {
        if (IsDeleted)
        {
            return;
        }

        IsRead = true;
        ReadAt ??= utcNow;
        UpdatedAt = utcNow;
    }

    public void Delete(DateTimeOffset utcNow)
    {
        IsDeleted = true;
        DeletedAt ??= utcNow;
        UpdatedAt = utcNow;
    }
}
