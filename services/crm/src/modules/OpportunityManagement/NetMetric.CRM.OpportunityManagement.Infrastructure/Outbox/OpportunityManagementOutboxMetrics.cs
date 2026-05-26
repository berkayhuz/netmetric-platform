// <copyright file="OpportunityManagementOutboxMetrics.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics.Metrics;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Outbox;

public sealed class OpportunityManagementOutboxMetrics
{
    private static readonly Meter Meter = new("NetMetric.CRM.OpportunityManagement.Outbox", "1.0.0");
    private readonly Counter<long> publishedCounter = Meter.CreateCounter<long>("opportunity_outbox_published_total");
    private readonly Counter<long> failedCounter = Meter.CreateCounter<long>("opportunity_outbox_failed_total");
    private readonly Counter<long> deadLetterCounter = Meter.CreateCounter<long>("opportunity_outbox_deadletter_total");
    private long backlog;

    public const string MeterName = "NetMetric.CRM.OpportunityManagement.Outbox";

    public OpportunityManagementOutboxMetrics()
    {
        Meter.CreateObservableGauge<long>(
            "opportunity_outbox_backlog",
            () => backlog,
            description: "opportunity outbox pending message backlog.");
    }

    public void SetBacklog(long value) => Interlocked.Exchange(ref backlog, value);
    public void Published() => publishedCounter.Add(1);
    public void Failed() => failedCounter.Add(1);
    public void DeadLettered() => deadLetterCounter.Add(1);
}
