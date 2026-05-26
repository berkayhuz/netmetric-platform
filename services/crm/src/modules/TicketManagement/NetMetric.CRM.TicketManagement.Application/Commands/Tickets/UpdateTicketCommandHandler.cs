// <copyright file="UpdateTicketCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Services;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketManagement.Application.Commands.Tickets;

public sealed class UpdateTicketCommandHandler(ITicketAdministrationService administrationService) : IRequestHandler<UpdateTicketCommand, TicketDetailDto>
{
    public Task<TicketDetailDto> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
        => administrationService.UpdateAsync(request, cancellationToken);
}
