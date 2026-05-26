// <copyright file="AnalyticsProjectionResult.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.AnalyticsReporting.Application.Results;

public sealed record AnalyticsProjectionResult(
    string CorrelationId,
    bool Succeeded,
    int ProjectedTenantCount,
    string? ErrorMessage);
