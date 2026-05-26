// <copyright file="UpsertLeadQualificationCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.LeadManagement.Application.Commands.Leads;

public sealed class UpsertLeadQualificationCommandHandler(ILeadAdministrationService administrationService)
    : IRequestHandler<UpsertLeadQualificationCommand>
{
    public async Task Handle(UpsertLeadQualificationCommand request, CancellationToken cancellationToken)
    {
        await administrationService.UpsertQualificationAsync(request, cancellationToken);
    }
}
