// <copyright file="CreateTicketCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Services;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketManagement.Application.Commands.Tickets;

public sealed class CreateTicketCommandHandler(ITicketAdministrationService administrationService) : IRequestHandler<CreateTicketCommand, TicketDetailDto>
{
    public Task<TicketDetailDto> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        => administrationService.CreateAsync(request, cancellationToken);
}
