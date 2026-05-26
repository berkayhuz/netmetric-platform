// <copyright file="GuidedSellingPlaybookUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.Requests;

public sealed class GuidedSellingPlaybookUpsertRequest
{
    public string Name { get; set; } = null!;
    public string? Segment { get; set; }
    public string? Industry { get; set; }
    public decimal? MinimumBudget { get; set; }
    public decimal? MaximumBudget { get; set; }
    public string? RequiredCapabilities { get; set; }
    public List<string> RecommendedBundleCodes { get; set; } = new();
    public string? QualificationJson { get; set; }
    public string? RowVersion { get; set; }
}
