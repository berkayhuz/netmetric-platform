// <copyright file="CalendarSyncRun.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.CalendarSync.Domain.Enums;
using NetMetric.Entities;

namespace NetMetric.CRM.CalendarSync.Domain.Entities;

public sealed class CalendarSyncRun : AuditableEntity
{
    private CalendarSyncRun()
    {
    }

    public CalendarSyncRun(Guid connectionId, CalendarSyncRunStatus status, int importedCount, int exportedCount, string? errorMessage)
    {
        ConnectionId = connectionId;
        Status = status;
        ImportedCount = importedCount;
        ExportedCount = exportedCount;
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage.Trim();
    }

    public Guid ConnectionId { get; private set; }
    public CalendarSyncRunStatus Status { get; private set; }
    public int ImportedCount { get; private set; }
    public int ExportedCount { get; private set; }
    public string? ErrorMessage { get; private set; }
}
