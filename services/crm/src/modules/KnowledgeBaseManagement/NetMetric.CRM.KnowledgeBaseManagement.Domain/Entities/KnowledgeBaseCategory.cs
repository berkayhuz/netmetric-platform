// <copyright file="KnowledgeBaseCategory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.KnowledgeBaseManagement.Domain.Entities;

public sealed class KnowledgeBaseCategory : AuditableEntity
{
    private KnowledgeBaseCategory() { }

    public KnowledgeBaseCategory(string name, string? description, int sortOrder)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SortOrder = sortOrder;
        Slug = BuildSlug(name);
    }

    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }

    public void Update(string name, string? description, int sortOrder)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SortOrder = sortOrder;
        Slug = BuildSlug(name);
    }

    private static string BuildSlug(string value)
        => value.Trim().ToLowerInvariant().Replace(" ", "-", StringComparison.Ordinal).Replace("--", "-", StringComparison.Ordinal);
}
