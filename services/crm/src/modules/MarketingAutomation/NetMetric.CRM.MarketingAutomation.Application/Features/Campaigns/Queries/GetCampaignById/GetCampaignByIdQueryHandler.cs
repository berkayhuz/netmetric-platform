// <copyright file="GetCampaignByIdQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.MarketingAutomation.Contracts.DTOs;

namespace NetMetric.CRM.MarketingAutomation.Application.Features.Campaigns.Queries.GetCampaignById;

public sealed class GetCampaignByIdQueryHandler(IMarketingAutomationDbContext dbContext)
    : IRequestHandler<GetCampaignByIdQuery, MarketingAutomationSummaryDto?>
{
    public async Task<MarketingAutomationSummaryDto?> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Campaigns
            .Where(x => x.Id == request.Id)
            .Select(x => new MarketingAutomationSummaryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}
