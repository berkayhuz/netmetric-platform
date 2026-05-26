// <copyright file="ToolCategory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Tools.Domain.Enums;
using NetMetric.Tools.Domain.ValueObjects;

namespace NetMetric.Tools.Domain.Entities;

public sealed class ToolCategory
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Slug { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public int SortOrder { get; private set; }

    private ToolCategory()
    {
        Slug = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
    }

    public ToolCategory(string slug, string title, string description, int sortOrder)
    {
        Slug = new ToolSlug(slug).Value;
        Title = Require(title, nameof(title));
        Description = Require(description, nameof(description));
        SortOrder = sortOrder;
    }

    private static string Require(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} is required.", name);
        }

        return value.Trim();
    }
}
