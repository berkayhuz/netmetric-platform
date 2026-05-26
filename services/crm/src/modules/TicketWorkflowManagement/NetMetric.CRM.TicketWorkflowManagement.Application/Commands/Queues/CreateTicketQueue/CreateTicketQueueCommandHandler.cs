// <copyright file="CreateTicketQueueCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketWorkflowManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Queues.CreateTicketQueue;

public sealed class CreateTicketQueueCommandHandler : IRequestHandler<CreateTicketQueueCommand, Guid>
{
    private readonly ITicketWorkflowManagementDbContext _dbContext;

    public CreateTicketQueueCommandHandler(ITicketWorkflowManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateTicketQueueCommand request, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.TicketQueues.AnyAsync(x => x.Code == request.Code, cancellationToken);
        if (exists)
            throw new InvalidOperationException("Ticket queue code already exists.");

        var queue = new TicketQueue(request.Code, request.Name, request.AssignmentStrategy);
        queue.Update(request.Name, request.Description, request.AssignmentStrategy, request.IsDefault);

        await _dbContext.TicketQueues.AddAsync(queue, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return queue.Id;
    }
}
