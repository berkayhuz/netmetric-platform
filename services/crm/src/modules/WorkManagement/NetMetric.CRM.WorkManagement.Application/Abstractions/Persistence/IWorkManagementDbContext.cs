// <copyright file="IWorkManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkManagement.Domain.Entities;

namespace NetMetric.CRM.WorkManagement.Application.Abstractions.Persistence;

public interface IWorkManagementDbContext
{
    DbSet<WorkTask> Tasks { get; }
    DbSet<ActivityLog> Activities { get; }
    DbSet<MeetingSchedule> Meetings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
