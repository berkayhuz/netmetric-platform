// <copyright file="ChangeLeadStatusCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.LeadManagement.Application.Commands.Leads;

public sealed class ChangeLeadStatusCommandHandler(ILeadAdministrationService administrationService)
    : IRequestHandler<ChangeLeadStatusCommand, Unit>
{
    public async Task<Unit> Handle(ChangeLeadStatusCommand request, CancellationToken cancellationToken)
    {
        await administrationService.ChangeStatusAsync(request, cancellationToken);
        return Unit.Value;
    }
}
