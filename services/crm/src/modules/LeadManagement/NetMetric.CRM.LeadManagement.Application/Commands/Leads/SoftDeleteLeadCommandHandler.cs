// <copyright file="SoftDeleteLeadCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.LeadManagement.Application.Commands.Leads;

public sealed class SoftDeleteLeadCommandHandler(ILeadAdministrationService administrationService)
    : IRequestHandler<SoftDeleteLeadCommand, Unit>
{
    public async Task<Unit> Handle(SoftDeleteLeadCommand request, CancellationToken cancellationToken)
    {
        await administrationService.SoftDeleteAsync(request, cancellationToken);
        return Unit.Value;
    }
}
