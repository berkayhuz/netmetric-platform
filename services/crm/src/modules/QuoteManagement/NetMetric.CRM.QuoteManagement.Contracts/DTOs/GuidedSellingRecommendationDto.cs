// <copyright file="GuidedSellingRecommendationDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.DTOs;

public sealed record GuidedSellingRecommendationDto(
    string PlaybookName,
    Guid BundleId,
    string BundleCode,
    string BundleName,
    decimal EstimatedDiscountRate,
    decimal Score,
    IReadOnlyList<string> Reasons);
