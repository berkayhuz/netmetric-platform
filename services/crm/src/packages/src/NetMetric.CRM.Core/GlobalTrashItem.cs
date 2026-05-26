// <copyright file="GlobalTrashItem.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Core;

public sealed class GlobalTrashItem : AuditableEntity
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string SourceModule { get; set; } = string.Empty;
    public string? OriginalRoute { get; set; }
    public DateTime DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public string? DeletedByDisplayName { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public string Status { get; set; } = CrmTrashStatuses.Active;
    public string? MetadataJson { get; set; }
    public string? AuditCorrelationId { get; set; }
    public DateTime? RestoredAtUtc { get; set; }
    public Guid? RestoredByUserId { get; set; }
    public DateTime? PurgedAtUtc { get; set; }
}

public static class CrmTrashEntityTypes
{
    public const string Contact = "contact";
    public const string Customer = "customer";
    public const string Company = "company";
    public const string Lead = "lead";
    public const string Deal = "deal";
    public const string Opportunity = "opportunity";
    public const string Quote = "quote";
    public const string Ticket = "ticket";
    public const string ProductCatalogItem = "productCatalogItem";
}

public static class CrmTrashStatuses
{
    public const string Active = "active";
    public const string Restored = "restored";
    public const string Purged = "purged";
}
