// <copyright file="GetLostReasonsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Queries;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class GetLostReasonsQueryHandler(
    IPipelineManagementDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetLostReasonsQuery, IReadOnlyList<LostReasonDto>>
{
    public async Task<IReadOnlyList<LostReasonDto>> Handle(GetLostReasonsQuery request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.EnsureTenant();

        return await dbContext.LostReasons
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);
    }
}
