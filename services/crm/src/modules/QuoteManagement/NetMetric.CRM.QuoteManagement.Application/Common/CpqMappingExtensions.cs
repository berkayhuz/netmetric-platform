// <copyright file="CpqMappingExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.QuoteManagement.Contracts.DTOs;
using NetMetric.CRM.QuoteManagement.Domain.Entities;

namespace NetMetric.CRM.QuoteManagement.Application.Common;

public static class CpqMappingExtensions
{
    public static ProductRuleDto ToDto(this ProductRule entity)
        => new(
            entity.Id,
            entity.Name,
            entity.RuleType,
            entity.TriggerProductId,
            entity.TargetProductId,
            entity.MinimumQuantity,
            entity.MaximumDiscountRate,
            entity.Severity,
            entity.Message,
            entity.CriteriaJson,
            entity.IsActive,
            Convert.ToBase64String(entity.RowVersion));

    public static ProductBundleDto ToDto(this ProductBundle entity)
        => new(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Description,
            entity.Segment,
            entity.Industry,
            entity.DiscountRate,
            entity.MinimumBudget,
            entity.Items.Select(x => new ProductBundleItemDto(x.ProductId, x.Quantity, x.IsOptional)).ToList(),
            entity.IsActive,
            Convert.ToBase64String(entity.RowVersion));

    public static GuidedSellingPlaybookDto ToDto(this GuidedSellingPlaybook entity)
        => new(
            entity.Id,
            entity.Name,
            entity.Segment,
            entity.Industry,
            entity.MinimumBudget,
            entity.MaximumBudget,
            entity.RequiredCapabilities,
            Split(entity.RecommendedBundleCodes),
            entity.QualificationJson,
            entity.IsActive,
            Convert.ToBase64String(entity.RowVersion));

    public static IReadOnlyList<string> Split(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
