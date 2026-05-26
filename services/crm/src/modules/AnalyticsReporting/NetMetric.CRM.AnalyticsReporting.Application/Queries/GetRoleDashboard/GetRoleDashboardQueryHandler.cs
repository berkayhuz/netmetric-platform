// <copyright file="GetRoleDashboardQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Persistence;
using NetMetric.CRM.AnalyticsReporting.Application.DTOs;

namespace NetMetric.CRM.AnalyticsReporting.Application.Queries.GetRoleDashboard;

public sealed class GetRoleDashboardQueryHandler(IAnalyticsReportingDbContext dbContext)
    : IRequestHandler<GetRoleDashboardQuery, DashboardSummaryDto>
{
    public async Task<DashboardSummaryDto> Handle(GetRoleDashboardQuery request, CancellationToken cancellationToken)
    {
        var widgets = await dbContext.DashboardWidgets
            .Where(x => x.RoleName == request.RoleName && x.IsEnabled)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new WidgetDto(x.WidgetKey, x.Title, x.DataSource, x.ConfigurationJson, x.DisplayOrder))
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto(request.RoleName, widgets);
    }
}
