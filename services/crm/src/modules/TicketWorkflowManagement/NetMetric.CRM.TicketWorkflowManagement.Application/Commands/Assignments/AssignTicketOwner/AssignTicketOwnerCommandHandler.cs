// <copyright file="AssignTicketOwnerCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Clock;
using NetMetric.CRM.TicketWorkflowManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Assignments.AssignTicketOwner;

public sealed class AssignTicketOwnerCommandHandler : IRequestHandler<AssignTicketOwnerCommand>
{
    private readonly ITicketWorkflowManagementDbContext _dbContext;
    private readonly IClock _clock;
    private readonly ICurrentUserService _currentUser;

    public AssignTicketOwnerCommandHandler(
        ITicketWorkflowManagementDbContext dbContext,
        IClock clock,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task Handle(AssignTicketOwnerCommand request, CancellationToken cancellationToken)
    {
        var history = new TicketAssignmentHistory(
            request.TicketId,
            previousOwnerUserId: request.PreviousOwnerUserId,
            newOwnerUserId: request.NewOwnerUserId,
            previousQueueId: request.QueueId,
            newQueueId: request.QueueId,
            changedByUserId: _currentUser.IsAuthenticated ? _currentUser.UserId : null,
            reason: request.Reason,
            changedAtUtc: _clock.UtcDateTime);

        await _dbContext.TicketAssignmentHistories.AddAsync(history, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
