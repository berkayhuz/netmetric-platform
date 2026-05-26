// <copyright file="ReportDefinition.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.AnalyticsReporting.Domain.Entities;

public sealed class ReportDefinition : EntityBase
{
    public string Name { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public string QueryKey { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsTenantScoped { get; private set; } = true;
    public bool IsEnabled { get; private set; } = true;

    private ReportDefinition() { }

    public ReportDefinition(string name, string category, string queryKey, string? description)
    {
        Name = name.Trim();
        Category = category.Trim();
        QueryKey = queryKey.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void Disable() => IsEnabled = false;
    public void Enable() => IsEnabled = true;
}
