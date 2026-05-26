// <copyright file="ICalendarSyncDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CalendarSync.Domain.Entities;

namespace NetMetric.CRM.CalendarSync.Application.Abstractions.Persistence;

public interface ICalendarSyncDbContext
{
    DbSet<CalendarConnection> Connections { get; }
    DbSet<CalendarEventBridge> EventBridges { get; }
    DbSet<CalendarSyncRun> SyncRuns { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
