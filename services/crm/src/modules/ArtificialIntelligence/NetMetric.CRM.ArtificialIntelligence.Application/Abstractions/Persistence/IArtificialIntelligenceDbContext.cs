// <copyright file="IArtificialIntelligenceDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ArtificialIntelligence.Domain.Entities;

namespace NetMetric.CRM.ArtificialIntelligence.Application.Abstractions.Persistence;

public interface IArtificialIntelligenceDbContext
{
    DbSet<AiProviderConnection> ProviderConnections { get; }
    DbSet<AiAutomationPolicy> AutomationPolicies { get; }
    DbSet<AiInsightRun> InsightRuns { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
