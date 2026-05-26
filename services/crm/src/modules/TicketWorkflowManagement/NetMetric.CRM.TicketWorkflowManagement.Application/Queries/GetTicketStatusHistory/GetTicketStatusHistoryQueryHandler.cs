// <copyright file="GetTicketStatusHistoryQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketWorkflowManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketWorkflowManagement.Application.DTOs;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Queries.GetTicketStatusHistory;

public sealed class GetTicketStatusHistoryQueryHandler : IRequestHandler<GetTicketStatusHistoryQuery, IReadOnlyList<TicketStatusHistoryDto>>
{
    private readonly ITicketWorkflowManagementDbContext _dbContext;

    public GetTicketStatusHistoryQueryHandler(ITicketWorkflowManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TicketStatusHistoryDto>> Handle(GetTicketStatusHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.TicketStatusHistories
            .Where(x => x.TicketId == request.TicketId)
            .OrderByDescending(x => x.ChangedAtUtc)
            .Select(x => new TicketStatusHistoryDto
            {
                Id = x.Id,
                TicketId = x.TicketId,
                PreviousStatus = x.PreviousStatus,
                NewStatus = x.NewStatus,
                ChangedByUserId = x.ChangedByUserId,
                Note = x.Note,
                ChangedAtUtc = x.ChangedAtUtc
            })
            .ToListAsync(cancellationToken);
    }
}
