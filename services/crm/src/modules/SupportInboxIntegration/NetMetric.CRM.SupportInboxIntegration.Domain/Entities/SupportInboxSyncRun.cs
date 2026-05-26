// <copyright file="SupportInboxSyncRun.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.SupportInboxIntegration.Domain.Enums;
using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.SupportInboxIntegration.Domain.Entities;

public sealed class SupportInboxSyncRun : AuditableEntity
{
    private SupportInboxSyncRun() { }

    public SupportInboxSyncRun(Guid connectionId, bool dryRun, DateTime startedAtUtc)
    {
        ConnectionId = connectionId;
        DryRun = dryRun;
        StartedAtUtc = startedAtUtc;
        Status = SupportInboxSyncRunStatus.Started;
    }

    public Guid ConnectionId { get; private set; }
    public bool DryRun { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public SupportInboxSyncRunStatus Status { get; private set; }
    public int PulledCount { get; private set; }
    public int CreatedTicketsCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    public void Complete(DateTime completedAtUtc, int pulledCount, int createdTicketsCount)
    {
        CompletedAtUtc = completedAtUtc;
        PulledCount = pulledCount;
        CreatedTicketsCount = createdTicketsCount;
        Status = SupportInboxSyncRunStatus.Completed;
        ErrorMessage = null;
    }

    public void Fail(DateTime completedAtUtc, string errorMessage)
    {
        CompletedAtUtc = completedAtUtc;
        Status = SupportInboxSyncRunStatus.Failed;
        ErrorMessage = Guard.AgainstNullOrWhiteSpace(errorMessage);
    }
}
