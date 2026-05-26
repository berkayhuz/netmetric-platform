// <copyright file="QuoteManagementOutboxMetrics.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Outbox;

public sealed class QuoteManagementOutboxMetrics
{
    private long backlog;
    private long published;
    private long failed;
    private long deadLettered;

    public long Backlog => Interlocked.Read(ref backlog);
    public long Published => Interlocked.Read(ref published);
    public long Failed => Interlocked.Read(ref failed);
    public long DeadLettered => Interlocked.Read(ref deadLettered);

    public void SetBacklog(long value) => Interlocked.Exchange(ref backlog, value);

    public void PublishedMessage() => Interlocked.Increment(ref published);

    public void FailedMessage() => Interlocked.Increment(ref failed);

    public void DeadLetteredMessage() => Interlocked.Increment(ref deadLettered);
}
