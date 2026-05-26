// <copyright file="AssignTicketOwnerCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.TicketManagement.Application.Commands.Tickets;

public sealed class AssignTicketOwnerCommandHandler(ITicketAdministrationService administrationService) : IRequestHandler<AssignTicketOwnerCommand>
{
    public async Task<Unit> Handle(AssignTicketOwnerCommand request, CancellationToken cancellationToken)
    {
        await administrationService.AssignOwnerAsync(request, cancellationToken);
        return Unit.Value;
    }

    Task IRequestHandler<AssignTicketOwnerCommand>.Handle(AssignTicketOwnerCommand request, CancellationToken cancellationToken)
    {
        return Handle(request, cancellationToken);
    }
}
