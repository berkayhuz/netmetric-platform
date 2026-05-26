// <copyright file="CustomerManagementOutboxMetrics.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics.Metrics;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Outbox;

public sealed class CustomerManagementOutboxMetrics
{
    public const string MeterName = "NetMetric.CRM.CustomerManagement.Outbox";

    private readonly Counter<long> publishedCounter;
    private readonly Counter<long> failedCounter;
    private readonly Counter<long> deadLetteredCounter;
    private readonly ObservableGauge<long> backlogGauge;
    private long backlog;

    public CustomerManagementOutboxMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        publishedCounter = meter.CreateCounter<long>("crm_customer_management_outbox_published_total");
        failedCounter = meter.CreateCounter<long>("crm_customer_management_outbox_publish_failed_total");
        deadLetteredCounter = meter.CreateCounter<long>("crm_customer_management_outbox_dead_lettered_total");
        backlogGauge = meter.CreateObservableGauge("crm_customer_management_outbox_backlog", () => Volatile.Read(ref backlog));
    }

    public void Published() => publishedCounter.Add(1);

    public void Failed() => failedCounter.Add(1);

    public void DeadLettered() => deadLetteredCounter.Add(1);

    public void SetBacklog(long value) => Volatile.Write(ref backlog, value);
}
