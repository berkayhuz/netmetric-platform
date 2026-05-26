// <copyright file="GuidedSellingPlaybook.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.QuoteManagement.Domain.Entities;

public sealed class GuidedSellingPlaybook : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Segment { get; set; }
    public string? Industry { get; set; }
    public decimal? MinimumBudget { get; set; }
    public decimal? MaximumBudget { get; set; }
    public string? RequiredCapabilities { get; set; }
    public string RecommendedBundleCodes { get; set; } = string.Empty;
    public string? QualificationJson { get; set; }
}
