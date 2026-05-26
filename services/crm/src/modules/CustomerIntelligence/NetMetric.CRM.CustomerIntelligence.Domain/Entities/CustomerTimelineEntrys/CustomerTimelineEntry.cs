// <copyright file="CustomerTimelineEntry.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerTimelineEntrys;

/// <summary>
/// Unified 360-degree timeline item.
/// </summary>
public sealed class CustomerTimelineEntry : AuditableEntity
{
    public CustomerTimelineEntry()
    {
    }

    public string SubjectType { get; set; } = "Customer";
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string? Channel { get; set; }
    public string? EntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? DataJson { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public static CustomerTimelineEntry Create(string name, string? entityType = null, Guid? relatedEntityId = null, string? dataJson = null)
    {
        return new CustomerTimelineEntry
        {
            Name = Guard.AgainstNullOrWhiteSpace(name),
            EntityType = string.IsNullOrWhiteSpace(entityType) ? null : entityType.Trim(),
            RelatedEntityId = relatedEntityId,
            DataJson = string.IsNullOrWhiteSpace(dataJson) ? null : dataJson.Trim(),
            OccurredAtUtc = DateTime.UtcNow
        };
    }
}
