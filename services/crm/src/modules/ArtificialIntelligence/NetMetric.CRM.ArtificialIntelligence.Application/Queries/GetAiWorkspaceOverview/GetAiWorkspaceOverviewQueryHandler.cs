// <copyright file="GetAiWorkspaceOverviewQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ArtificialIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.ArtificialIntelligence.Contracts.DTOs;
using NetMetric.CRM.ArtificialIntelligence.Domain.Enums;

namespace NetMetric.CRM.ArtificialIntelligence.Application.Queries.GetAiWorkspaceOverview;

public sealed class GetAiWorkspaceOverviewQueryHandler(IArtificialIntelligenceDbContext dbContext) : IRequestHandler<GetAiWorkspaceOverviewQuery, AiWorkspaceOverviewDto>
{
    public async Task<AiWorkspaceOverviewDto> Handle(GetAiWorkspaceOverviewQuery request, CancellationToken cancellationToken)
    {
        var providerRows = await dbContext.ProviderConnections
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var providers = providerRows
            .Select(x => new AiProviderConnectionDto(
                x.Id,
                x.Name,
                x.Provider.ToString(),
                x.ModelName,
                x.Endpoint,
                x.Capabilities.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                x.IsActive))
            .ToList();

        var completedRunCount = await dbContext.InsightRuns.CountAsync(x => x.Status == AiRunStatus.Completed, cancellationToken);
        var failedRunCount = await dbContext.InsightRuns.CountAsync(x => x.Status == AiRunStatus.Failed, cancellationToken);
        var automationPolicyCount = await dbContext.AutomationPolicies.CountAsync(cancellationToken);

        return new AiWorkspaceOverviewDto(
            providers.Count(x => x.IsActive),
            automationPolicyCount,
            completedRunCount,
            failedRunCount,
            providers);
    }
}
