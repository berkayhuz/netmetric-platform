// <copyright file="ChangeTicketStatusCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.TicketManagement.Application.Commands.Tickets;

public sealed class ChangeTicketStatusCommandHandler(ITicketAdministrationService administrationService) : IRequestHandler<ChangeTicketStatusCommand>
{
    public async Task<Unit> Handle(ChangeTicketStatusCommand request, CancellationToken cancellationToken)
    {
        await administrationService.ChangeStatusAsync(request, cancellationToken);
        return Unit.Value;
    }

    Task IRequestHandler<ChangeTicketStatusCommand>.Handle(ChangeTicketStatusCommand request, CancellationToken cancellationToken)
    {
        return Handle(request, cancellationToken);
    }
}
