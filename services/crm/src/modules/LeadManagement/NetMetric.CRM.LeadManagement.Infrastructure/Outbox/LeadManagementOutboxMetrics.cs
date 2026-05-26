// <copyright file="LeadManagementOutboxMetrics.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics.Metrics;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Outbox;

public sealed class LeadManagementOutboxMetrics
{
    private static readonly Meter Meter = new("NetMetric.CRM.LeadManagement.Outbox", "1.0.0");
    private readonly Counter<long> publishedCounter = Meter.CreateCounter<long>("lead_management_outbox_published_total");
    private readonly Counter<long> failedCounter = Meter.CreateCounter<long>("lead_management_outbox_failed_total");
    private readonly Counter<long> deadLetteredCounter = Meter.CreateCounter<long>("lead_management_outbox_dead_lettered_total");
    private readonly UpDownCounter<long> backlogGauge = Meter.CreateUpDownCounter<long>("lead_management_outbox_backlog");

    public void Published() => publishedCounter.Add(1);
    public void Failed() => failedCounter.Add(1);
    public void DeadLettered() => deadLetteredCounter.Add(1);
    public void SetBacklog(int value) => backlogGauge.Add(value);
}
