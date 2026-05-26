// <copyright file="TagMap.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Tagging;

public class TagMap : AuditableEntity
{
    public Guid TagId { get; private set; }
    public Tag? Tag { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }

    private TagMap()
    {
    }

    public TagMap(Guid tagId, EntityReference entity)
    {
        if (tagId == Guid.Empty)
            throw new ArgumentException("TagId cannot be empty.", nameof(tagId));

        if (entity.EntityId == Guid.Empty)
            throw new ArgumentException("EntityId cannot be empty.", nameof(entity));

        if (string.IsNullOrWhiteSpace(entity.EntityType))
            throw new ArgumentException("EntityType cannot be empty.", nameof(entity));

        var normalizedEntityType = entity.EntityType.Trim().ToLowerInvariant();
        if (!CrmEntityTypes.IsSupported(normalizedEntityType))
            throw new ArgumentException($"Unsupported CRM entity type '{entity.EntityType}'.", nameof(entity));

        TagId = tagId;
        EntityType = normalizedEntityType;
        EntityId = entity.EntityId;
    }

    public void ReassignEntity(EntityReference entity)
    {
        if (entity.EntityId == Guid.Empty)
            throw new ArgumentException("EntityId cannot be empty.", nameof(entity));

        if (string.IsNullOrWhiteSpace(entity.EntityType))
            throw new ArgumentException("EntityType cannot be empty.", nameof(entity));

        var normalizedEntityType = entity.EntityType.Trim().ToLowerInvariant();
        if (!CrmEntityTypes.IsSupported(normalizedEntityType))
            throw new ArgumentException($"Unsupported CRM entity type '{entity.EntityType}'.", nameof(entity));

        EntityType = normalizedEntityType;
        EntityId = entity.EntityId;
    }
}
