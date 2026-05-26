// <copyright file="BehavioralEvent.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.CustomerIntelligence.Domain.Entities.BehavioralEvents;

public sealed class BehavioralEvent : AuditableEntity
{
    public string Source { get; set; } = null!;
    public string EventName { get; set; } = null!;
    public string SubjectType { get; set; } = null!;
    public Guid SubjectId { get; set; }
    public string? IdentityKey { get; set; }
    public string? Channel { get; set; }
    public string? PropertiesJson { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}
