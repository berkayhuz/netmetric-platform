// <copyright file="AttachSlaToTicketCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.TicketSla;

public sealed class AttachSlaToTicketCommandHandler(ITicketSlaAdministrationService service) : IRequestHandler<AttachSlaToTicketCommand>
{
    public Task Handle(AttachSlaToTicketCommand request, CancellationToken cancellationToken) =>
        service.AttachPolicyToTicketAsync(request.TicketId, request.SlaPolicyId, request.CreatedAtUtc, cancellationToken);
}
