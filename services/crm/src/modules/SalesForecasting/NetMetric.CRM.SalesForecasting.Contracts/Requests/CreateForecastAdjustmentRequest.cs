// <copyright file="CreateForecastAdjustmentRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SalesForecasting.Contracts.Requests;

public sealed record CreateForecastAdjustmentRequest(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    Guid? OwnerUserId,
    decimal Amount,
    string Reason,
    string? Notes);
