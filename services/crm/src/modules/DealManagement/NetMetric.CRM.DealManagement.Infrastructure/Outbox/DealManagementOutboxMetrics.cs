// <copyright file="DealManagementOutboxMetrics.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics.Metrics;

namespace NetMetric.CRM.DealManagement.Infrastructure.Outbox;

public sealed class DealManagementOutboxMetrics
{
    private static readonly Meter Meter = new("NetMetric.CRM.DealManagement.Outbox", "1.0.0");
    private readonly Counter<long> publishedCounter = Meter.CreateCounter<long>("deal_outbox_published_total");
    private readonly Counter<long> failedCounter = Meter.CreateCounter<long>("deal_outbox_failed_total");
    private readonly Counter<long> deadLetterCounter = Meter.CreateCounter<long>("deal_outbox_deadletter_total");
    private readonly ObservableGauge<long> backlogGauge;
    private long backlog;

    public const string MeterName = "NetMetric.CRM.DealManagement.Outbox";

    public DealManagementOutboxMetrics()
    {
        backlogGauge = Meter.CreateObservableGauge<long>(
            "deal_outbox_backlog",
            () => backlog,
            description: "deal outbox pending message backlog.");
    }

    public void SetBacklog(long value) => Interlocked.Exchange(ref backlog, value);

    public void Published() => publishedCounter.Add(1);

    public void Failed() => failedCounter.Add(1);

    public void DeadLettered() => deadLetterCounter.Add(1);
}

