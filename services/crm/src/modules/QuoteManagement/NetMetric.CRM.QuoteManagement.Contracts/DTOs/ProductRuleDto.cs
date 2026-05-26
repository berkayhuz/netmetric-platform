// <copyright file="ProductRuleDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.DTOs;

public sealed record ProductRuleDto(
    Guid Id,
    string Name,
    string RuleType,
    Guid? TriggerProductId,
    Guid? TargetProductId,
    int? MinimumQuantity,
    decimal? MaximumDiscountRate,
    string Severity,
    string Message,
    string? CriteriaJson,
    bool IsActive,
    string RowVersion);
