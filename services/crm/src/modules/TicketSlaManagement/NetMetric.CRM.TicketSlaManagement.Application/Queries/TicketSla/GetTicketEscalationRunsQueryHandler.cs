// <copyright file="GetTicketEscalationRunsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketSlaManagement.Application.Common;
using NetMetric.CRM.TicketSlaManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketSlaManagement.Application.Queries.TicketSla;

public sealed class GetTicketEscalationRunsQueryHandler(ITicketSlaManagementDbContext dbContext) : IRequestHandler<GetTicketEscalationRunsQuery, IReadOnlyList<TicketEscalationRunDto>>
{
    public async Task<IReadOnlyList<TicketEscalationRunDto>> Handle(GetTicketEscalationRunsQuery request, CancellationToken cancellationToken) =>
        await dbContext.TicketEscalationRuns
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.TicketId == request.TicketId)
            .OrderByDescending(x => x.ExecutedAtUtc)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);
}
