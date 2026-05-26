// <copyright file="ISupportInboxIntegrationDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.SupportInboxIntegration.Domain.Entities;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Persistence;

public interface ISupportInboxIntegrationDbContext
{
    DbSet<SupportInboxConnection> Connections { get; }
    DbSet<SupportInboxRule> Rules { get; }
    DbSet<SupportInboxMessage> Messages { get; }
    DbSet<SupportInboxSyncRun> SyncRuns { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
