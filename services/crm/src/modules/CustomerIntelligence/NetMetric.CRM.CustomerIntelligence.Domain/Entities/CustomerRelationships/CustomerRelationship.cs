// <copyright file="CustomerRelationship.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerRelationships;

/// <summary>
/// Relationship mapping between customers/contacts/companies.
/// </summary>
public sealed class CustomerRelationship : AuditableEntity
{
    public CustomerRelationship()
    {
    }

    public string SourceEntityType { get; set; } = "Customer";
    public Guid SourceEntityId { get; set; }
    public string TargetEntityType { get; set; } = "Customer";
    public Guid TargetEntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = "Related";
    public decimal StrengthScore { get; set; }
    public bool IsBidirectional { get; set; } = true;
    public string? EntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? DataJson { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public static CustomerRelationship Create(string name, string? entityType = null, Guid? relatedEntityId = null, string? dataJson = null)
    {
        return new CustomerRelationship
        {
            Name = Guard.AgainstNullOrWhiteSpace(name),
            EntityType = string.IsNullOrWhiteSpace(entityType) ? null : entityType.Trim(),
            RelatedEntityId = relatedEntityId,
            DataJson = string.IsNullOrWhiteSpace(dataJson) ? null : dataJson.Trim(),
            OccurredAtUtc = DateTime.UtcNow
        };
    }
}
