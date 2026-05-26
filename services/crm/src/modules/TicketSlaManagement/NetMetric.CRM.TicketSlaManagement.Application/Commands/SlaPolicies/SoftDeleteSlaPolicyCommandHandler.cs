// <copyright file="SoftDeleteSlaPolicyCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.SlaPolicies;

public sealed class SoftDeleteSlaPolicyCommandHandler(ITicketSlaAdministrationService service) : IRequestHandler<SoftDeleteSlaPolicyCommand>
{
    public Task Handle(SoftDeleteSlaPolicyCommand request, CancellationToken cancellationToken) =>
        service.SoftDeletePolicyAsync(request.Id, cancellationToken);
}
