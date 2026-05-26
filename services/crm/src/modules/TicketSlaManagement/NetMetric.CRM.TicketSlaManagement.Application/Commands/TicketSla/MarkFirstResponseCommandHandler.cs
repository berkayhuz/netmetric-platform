// <copyright file="MarkFirstResponseCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.TicketSla;

public sealed class MarkFirstResponseCommandHandler(ITicketSlaAdministrationService service) : IRequestHandler<MarkFirstResponseCommand>
{
    public Task Handle(MarkFirstResponseCommand request, CancellationToken cancellationToken) =>
        service.MarkFirstResponseAsync(request.TicketId, request.RespondedAtUtc, cancellationToken);
}
