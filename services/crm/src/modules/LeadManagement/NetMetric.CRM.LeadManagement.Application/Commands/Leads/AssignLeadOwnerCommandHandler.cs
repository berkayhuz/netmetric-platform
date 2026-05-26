// <copyright file="AssignLeadOwnerCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.LeadManagement.Application.Commands.Leads;

public sealed class AssignLeadOwnerCommandHandler(ILeadAdministrationService administrationService)
    : IRequestHandler<AssignLeadOwnerCommand, Unit>
{
    public async Task<Unit> Handle(AssignLeadOwnerCommand request, CancellationToken cancellationToken)
    {
        await administrationService.AssignOwnerAsync(request, cancellationToken);
        return Unit.Value;
    }
}
