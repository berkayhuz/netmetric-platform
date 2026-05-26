// <copyright file="ActivityLog.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.WorkManagement.Domain.Enums;
using NetMetric.Entities;

namespace NetMetric.CRM.WorkManagement.Domain.Entities;

public sealed class ActivityLog : AuditableEntity
{
    private ActivityLog()
    {
    }

    public ActivityLog(WorkActivityType activityType, string subject, DateTime occurredAtUtc, Guid? relatedEntityId)
    {
        ActivityType = activityType;
        Subject = string.IsNullOrWhiteSpace(subject) ? throw new ArgumentException("Subject is required.", nameof(subject)) : subject.Trim();
        OccurredAtUtc = occurredAtUtc;
        RelatedEntityId = relatedEntityId;
    }

    public WorkActivityType ActivityType { get; private set; }
    public string Subject { get; private set; } = null!;
    public DateTime OccurredAtUtc { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
}
