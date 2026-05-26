// <copyright file="ScheduleNextContactCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.LeadManagement.Application.Commands.Leads;

public sealed class ScheduleNextContactCommandHandler(ILeadAdministrationService administrationService)
    : IRequestHandler<ScheduleNextContactCommand, Unit>
{
    public async Task<Unit> Handle(ScheduleNextContactCommand request, CancellationToken cancellationToken)
    {
        await administrationService.ScheduleNextContactAsync(request, cancellationToken);
        return Unit.Value;
    }
}
