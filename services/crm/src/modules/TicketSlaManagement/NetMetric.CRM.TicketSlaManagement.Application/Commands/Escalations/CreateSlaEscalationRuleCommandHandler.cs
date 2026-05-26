// <copyright file="CreateSlaEscalationRuleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.Escalations;

public sealed class CreateSlaEscalationRuleCommandHandler(ITicketSlaAdministrationService service) : IRequestHandler<CreateSlaEscalationRuleCommand, Guid>
{
    public Task<Guid> Handle(CreateSlaEscalationRuleCommand request, CancellationToken cancellationToken) =>
        service.CreateEscalationRuleAsync(request.ToEntity(), cancellationToken);
}
