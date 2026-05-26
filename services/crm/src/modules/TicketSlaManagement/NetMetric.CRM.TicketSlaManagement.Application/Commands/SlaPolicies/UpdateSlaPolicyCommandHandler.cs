// <copyright file="UpdateSlaPolicyCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.SlaPolicies;

public sealed class UpdateSlaPolicyCommandHandler(ITicketSlaAdministrationService service) : IRequestHandler<UpdateSlaPolicyCommand>
{
    public Task Handle(UpdateSlaPolicyCommand request, CancellationToken cancellationToken) =>
        service.UpdatePolicyAsync(
            request.Id,
            request.Name,
            request.TicketCategoryId,
            request.Priority,
            request.FirstResponseTargetMinutes,
            request.ResolutionTargetMinutes,
            request.IsDefault,
            cancellationToken);
}
