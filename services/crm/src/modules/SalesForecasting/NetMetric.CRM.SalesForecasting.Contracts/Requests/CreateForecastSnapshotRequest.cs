// <copyright file="CreateForecastSnapshotRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SalesForecasting.Contracts.Requests;

public sealed record CreateForecastSnapshotRequest(
    string Name,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    Guid? OwnerUserId,
    string ForecastCategory,
    string? Notes);
