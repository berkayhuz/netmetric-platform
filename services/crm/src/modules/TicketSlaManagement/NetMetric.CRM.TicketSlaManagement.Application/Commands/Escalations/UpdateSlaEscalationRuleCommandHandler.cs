// <copyright file="UpdateSlaEscalationRuleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.Escalations;

public sealed class UpdateSlaEscalationRuleCommandHandler(ITicketSlaAdministrationService service) : IRequestHandler<UpdateSlaEscalationRuleCommand>
{
    public Task Handle(UpdateSlaEscalationRuleCommand request, CancellationToken cancellationToken) =>
        service.UpdateEscalationRuleAsync(
            request.Id,
            request.SlaPolicyId,
            request.MetricType,
            request.TriggerBeforeOrAfterMinutes,
            request.ActionType,
            request.TargetTeamId,
            request.TargetUserId,
            request.IsEnabled,
            cancellationToken);
}
