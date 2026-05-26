// <copyright file="GetCampaignRoiQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Persistence;
using NetMetric.CRM.AnalyticsReporting.Application.DTOs;

namespace NetMetric.CRM.AnalyticsReporting.Application.Queries.GetCampaignRoi;

public sealed class GetCampaignRoiQueryHandler(IAnalyticsReportingDbContext dbContext)
    : IRequestHandler<GetCampaignRoiQuery, IReadOnlyCollection<CampaignRoiDto>>
{
    public async Task<IReadOnlyCollection<CampaignRoiDto>> Handle(GetCampaignRoiQuery request, CancellationToken cancellationToken)
    {
        var latestSnapshotAtUtc = await dbContext.CampaignRoiSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == request.TenantId)
            .MaxAsync(x => (DateTime?)x.SnapshotAtUtc, cancellationToken);

        if (latestSnapshotAtUtc is null)
        {
            return [];
        }

        return await dbContext.CampaignRoiSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == request.TenantId && x.SnapshotAtUtc == latestSnapshotAtUtc)
            .OrderByDescending(x => x.RoiPercentage)
            .Select(x => new CampaignRoiDto(x.CampaignName, x.Spend, x.Revenue, x.RoiPercentage))
            .ToListAsync(cancellationToken);
    }
}
