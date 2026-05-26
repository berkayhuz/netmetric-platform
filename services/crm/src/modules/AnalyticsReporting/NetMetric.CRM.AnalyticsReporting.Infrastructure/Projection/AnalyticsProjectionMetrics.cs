// <copyright file="AnalyticsProjectionMetrics.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics.Metrics;

namespace NetMetric.CRM.AnalyticsReporting.Infrastructure.Projection;

public static class AnalyticsProjectionMetrics
{
    public const string MeterName = "NetMetric.CRM.AnalyticsReporting";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> ProjectionRunCounter = Meter.CreateCounter<long>("crm_analytics_projection_runs_total");
    private static readonly Counter<long> ProjectionFailureCounter = Meter.CreateCounter<long>("crm_analytics_projection_failures_total");
    private static readonly Histogram<double> ProjectionDuration = Meter.CreateHistogram<double>("crm_analytics_projection_duration_ms");
    private static readonly Histogram<long> ProjectedTenantCount = Meter.CreateHistogram<long>("crm_analytics_projection_tenants");

    public static void RecordSuccess(TimeSpan duration, int tenantCount)
    {
        ProjectionRunCounter.Add(1);
        ProjectionDuration.Record(duration.TotalMilliseconds);
        ProjectedTenantCount.Record(tenantCount);
    }

    public static void RecordFailure(TimeSpan duration)
    {
        ProjectionRunCounter.Add(1);
        ProjectionFailureCounter.Add(1);
        ProjectionDuration.Record(duration.TotalMilliseconds);
    }
}
