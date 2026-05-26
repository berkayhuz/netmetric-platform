// <copyright file="MarkResolvedCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.TicketSla;

public sealed class MarkResolvedCommandHandler(ITicketSlaAdministrationService service) : IRequestHandler<MarkResolvedCommand>
{
    public Task Handle(MarkResolvedCommand request, CancellationToken cancellationToken) =>
        service.MarkResolvedAsync(request.TicketId, request.ResolvedAtUtc, cancellationToken);
}
