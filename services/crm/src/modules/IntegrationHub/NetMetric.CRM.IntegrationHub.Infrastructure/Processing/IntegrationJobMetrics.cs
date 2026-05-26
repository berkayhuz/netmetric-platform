// <copyright file="IntegrationJobMetrics.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics.Metrics;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Processing;

public static class IntegrationJobMetrics
{
    public const string MeterName = "NetMetric.CRM.IntegrationHub";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> JobsSucceeded = Meter.CreateCounter<long>("integration_jobs_succeeded_total");
    private static readonly Counter<long> JobsRetried = Meter.CreateCounter<long>("integration_jobs_retried_total");
    private static readonly Counter<long> JobsDeadLettered = Meter.CreateCounter<long>("integration_jobs_dead_lettered_total");
    private static readonly Counter<long> JobsCanceled = Meter.CreateCounter<long>("integration_jobs_canceled_total");

    public static void RecordSucceeded(string providerKey) => JobsSucceeded.Add(1, KeyValuePair.Create<string, object?>("provider", providerKey));
    public static void RecordRetried(string providerKey) => JobsRetried.Add(1, KeyValuePair.Create<string, object?>("provider", providerKey));
    public static void RecordDeadLettered(string providerKey) => JobsDeadLettered.Add(1, KeyValuePair.Create<string, object?>("provider", providerKey));
    public static void RecordCanceled(string providerKey) => JobsCanceled.Add(1, KeyValuePair.Create<string, object?>("provider", providerKey));
}
