// <copyright file="CreateCampaignCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.MarketingAutomation.Contracts.DTOs;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Campaigns;

namespace NetMetric.CRM.MarketingAutomation.Application.Features.Campaigns.Commands.CreateCampaign;

public sealed class CreateCampaignCommandHandler(IMarketingAutomationDbContext dbContext)
    : IRequestHandler<CreateCampaignCommand, MarketingAutomationSummaryDto>
{
    public async Task<MarketingAutomationSummaryDto> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var entity = new Campaign(request.Code, request.Name, request.Description);
        await dbContext.Campaigns.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new MarketingAutomationSummaryDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive
        };
    }
}
