// <copyright file="CalendarEventBridge.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.CalendarSync.Domain.Entities;

public sealed class CalendarEventBridge : AuditableEntity
{
    private CalendarEventBridge()
    {
    }

    public CalendarEventBridge(Guid connectionId, Guid internalMeetingId, string externalEventId, DateTime lastSynchronizedAtUtc)
    {
        ConnectionId = connectionId;
        InternalMeetingId = internalMeetingId;
        ExternalEventId = string.IsNullOrWhiteSpace(externalEventId) ? throw new ArgumentException("External event id is required.", nameof(externalEventId)) : externalEventId.Trim();
        LastSynchronizedAtUtc = lastSynchronizedAtUtc;
    }

    public Guid ConnectionId { get; private set; }
    public Guid InternalMeetingId { get; private set; }
    public string ExternalEventId { get; private set; } = null!;
    public DateTime LastSynchronizedAtUtc { get; private set; }
}
