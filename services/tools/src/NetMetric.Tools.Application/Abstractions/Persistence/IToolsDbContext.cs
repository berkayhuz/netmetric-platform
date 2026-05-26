// <copyright file="IToolsDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.Tools.Domain.Entities;

namespace NetMetric.Tools.Application.Abstractions.Persistence;

public interface IToolsDbContext
{
    DbSet<ToolCategory> ToolCategories { get; }
    DbSet<ToolDefinition> ToolDefinitions { get; }
    DbSet<ToolRun> ToolRuns { get; }
    DbSet<ToolArtifact> ToolArtifacts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
