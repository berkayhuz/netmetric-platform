// <copyright file="DocumentRecord.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentRecords;

/// <summary>
/// Document aggregate root.
/// </summary>
public sealed class DocumentRecord : AuditableEntity
{
    public DocumentRecord()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string? EntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public string? DataJson { get; private set; }
    public DateTime OccurredAtUtc { get; private set; } = DateTime.UtcNow;

    public static DocumentRecord Create(string name, string? entityType = null, Guid? relatedEntityId = null, string? dataJson = null)
    {
        return new DocumentRecord
        {
            Name = Guard.AgainstNullOrWhiteSpace(name),
            EntityType = string.IsNullOrWhiteSpace(entityType) ? null : entityType.Trim(),
            RelatedEntityId = relatedEntityId,
            DataJson = string.IsNullOrWhiteSpace(dataJson) ? null : dataJson.Trim(),
            OccurredAtUtc = DateTime.UtcNow
        };
    }
}
