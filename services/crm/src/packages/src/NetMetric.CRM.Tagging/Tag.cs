// <copyright file="Tag.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Tagging;

public class Tag : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? ColorHex
    {
        get => Color;
        set => Color = value;
    }
    public string? Description { get; set; }
    public ICollection<TagMap> TagMaps { get; set; } = [];
}
