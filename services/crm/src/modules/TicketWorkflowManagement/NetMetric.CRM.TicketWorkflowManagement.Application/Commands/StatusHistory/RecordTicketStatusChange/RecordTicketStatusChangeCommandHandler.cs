// <copyright file="RecordTicketStatusChangeCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Clock;
using NetMetric.CRM.TicketWorkflowManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Commands.StatusHistory.RecordTicketStatusChange;

public sealed class RecordTicketStatusChangeCommandHandler : IRequestHandler<RecordTicketStatusChangeCommand>
{
    private readonly ITicketWorkflowManagementDbContext _dbContext;
    private readonly IClock _clock;
    private readonly ICurrentUserService _currentUser;

    public RecordTicketStatusChangeCommandHandler(
        ITicketWorkflowManagementDbContext dbContext,
        IClock clock,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task Handle(RecordTicketStatusChangeCommand request, CancellationToken cancellationToken)
    {
        var history = new TicketStatusHistory(
            request.TicketId,
            request.PreviousStatus,
            request.NewStatus,
            _currentUser.IsAuthenticated ? _currentUser.UserId : null,
            request.Note,
            _clock.UtcDateTime);

        await _dbContext.TicketStatusHistories.AddAsync(history, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
