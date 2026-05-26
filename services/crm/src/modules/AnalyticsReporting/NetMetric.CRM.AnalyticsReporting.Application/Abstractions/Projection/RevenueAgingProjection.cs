// <copyright file="RevenueAgingProjection.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Projection;

public sealed record RevenueAgingProjection(
    Guid TenantId,
    decimal CurrentAmount,
    decimal Days30,
    decimal Days60,
    decimal Days90Plus);

