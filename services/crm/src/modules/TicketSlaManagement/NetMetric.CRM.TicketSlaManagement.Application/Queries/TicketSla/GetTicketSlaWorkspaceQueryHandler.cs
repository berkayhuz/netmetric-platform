// <copyright file="GetTicketSlaWorkspaceQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketSlaManagement.Application.Common;
using NetMetric.CRM.TicketSlaManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketSlaManagement.Application.Queries.TicketSla;

public sealed class GetTicketSlaWorkspaceQueryHandler(ITicketSlaManagementDbContext dbContext) : IRequestHandler<GetTicketSlaWorkspaceQuery, TicketSlaWorkspaceDto?>
{
    public async Task<TicketSlaWorkspaceDto?> Handle(GetTicketSlaWorkspaceQuery request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TicketSlaInstances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.TicketId == request.TicketId, cancellationToken);

        return entity?.ToDto();
    }
}
