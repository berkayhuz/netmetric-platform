// <copyright file="RunDueEscalationsCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.TicketSla;

public sealed class RunDueEscalationsCommandHandler(ITicketSlaAdministrationService service) : IRequestHandler<RunDueEscalationsCommand, int>
{
    public Task<int> Handle(RunDueEscalationsCommand request, CancellationToken cancellationToken) =>
        service.RunDueEscalationsAsync(request.UtcNow, cancellationToken);
}
