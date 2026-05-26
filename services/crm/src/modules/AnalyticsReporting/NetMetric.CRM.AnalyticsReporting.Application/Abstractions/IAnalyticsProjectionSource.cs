// <copyright file="IAnalyticsProjectionSource.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.AnalyticsReporting.Application.Batchs;

namespace NetMetric.CRM.AnalyticsReporting.Application.Abstractions;

public interface IAnalyticsProjectionSource
{
    Task<AnalyticsProjectionBatch> ReadAsync(CancellationToken cancellationToken);
}
