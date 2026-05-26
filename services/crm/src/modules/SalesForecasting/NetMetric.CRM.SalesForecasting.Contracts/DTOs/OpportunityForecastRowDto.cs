// <copyright file="OpportunityForecastRowDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.SalesForecasting.Contracts.Enums;

namespace NetMetric.CRM.SalesForecasting.Contracts.DTOs;

public sealed record OpportunityForecastRowDto(
    Guid OpportunityId,
    string OpportunityCode,
    string Name,
    Guid? OwnerUserId,
    DateTime? EstimatedCloseDate,
    decimal EstimatedAmount,
    decimal Probability,
    ForecastBucketType Bucket,
    decimal WeightedAmount);
