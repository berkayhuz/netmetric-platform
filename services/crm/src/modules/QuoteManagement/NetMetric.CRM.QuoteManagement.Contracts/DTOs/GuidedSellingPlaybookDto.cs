// <copyright file="GuidedSellingPlaybookDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.DTOs;

public sealed record GuidedSellingPlaybookDto(
    Guid Id,
    string Name,
    string? Segment,
    string? Industry,
    decimal? MinimumBudget,
    decimal? MaximumBudget,
    string? RequiredCapabilities,
    IReadOnlyList<string> RecommendedBundleCodes,
    string? QualificationJson,
    bool IsActive,
    string RowVersion);
