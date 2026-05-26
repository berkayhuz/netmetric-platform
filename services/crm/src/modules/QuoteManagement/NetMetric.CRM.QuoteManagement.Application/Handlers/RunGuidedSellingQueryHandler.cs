// <copyright file="RunGuidedSellingQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Application.Queries.Quotes;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class RunGuidedSellingQueryHandler(IQuoteManagementDbContext dbContext) : IRequestHandler<RunGuidedSellingQuery, IReadOnlyList<GuidedSellingRecommendationDto>>
{
    public async Task<IReadOnlyList<GuidedSellingRecommendationDto>> Handle(RunGuidedSellingQuery request, CancellationToken cancellationToken)
    {
        var bundles = await dbContext.ProductBundles.Include(x => x.Items).AsNoTracking().Where(x => x.IsActive).ToListAsync(cancellationToken);
        var playbooks = await dbContext.GuidedSellingPlaybooks.AsNoTracking().Where(x => x.IsActive).ToListAsync(cancellationToken);
        var requiredCapabilities = request.RequiredCapabilities
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var recommendations = new List<GuidedSellingRecommendationDto>();

        foreach (var playbook in playbooks)
        {
            var bundleCodes = CpqMappingExtensions.Split(playbook.RecommendedBundleCodes);
            foreach (var bundle in bundles.Where(x => bundleCodes.Contains(x.Code, StringComparer.OrdinalIgnoreCase)))
            {
                var reasons = new List<string>();
                decimal score = 0;

                if (!string.IsNullOrWhiteSpace(request.Segment) && string.Equals(request.Segment, playbook.Segment, StringComparison.OrdinalIgnoreCase))
                {
                    score += 35;
                    reasons.Add($"Segment matched {playbook.Segment}.");
                }

                if (!string.IsNullOrWhiteSpace(request.Industry) && string.Equals(request.Industry, playbook.Industry, StringComparison.OrdinalIgnoreCase))
                {
                    score += 25;
                    reasons.Add($"Industry matched {playbook.Industry}.");
                }

                if (!request.Budget.HasValue || !playbook.MinimumBudget.HasValue || request.Budget.Value >= playbook.MinimumBudget.Value)
                {
                    score += 20;
                    reasons.Add("Budget satisfies minimum threshold.");
                }

                if (!request.Budget.HasValue || !playbook.MaximumBudget.HasValue || request.Budget.Value <= playbook.MaximumBudget.Value)
                    score += 10;

                var playbookCapabilities = CpqMappingExtensions.Split(playbook.RequiredCapabilities);
                if (playbookCapabilities.Count == 0 || playbookCapabilities.All(requiredCapabilities.Contains))
                {
                    score += 10;
                    if (playbookCapabilities.Count > 0)
                        reasons.Add("Required capabilities are covered.");
                }

                if (score <= 0)
                    continue;

                recommendations.Add(new GuidedSellingRecommendationDto(
                    playbook.Name,
                    bundle.Id,
                    bundle.Code,
                    bundle.Name,
                    bundle.DiscountRate,
                    score,
                    reasons));
            }
        }

        return recommendations.OrderByDescending(x => x.Score).ThenBy(x => x.BundleName).Take(10).ToList();
    }
}
