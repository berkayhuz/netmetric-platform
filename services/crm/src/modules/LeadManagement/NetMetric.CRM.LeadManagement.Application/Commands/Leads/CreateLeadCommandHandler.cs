// <copyright file="CreateLeadCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;

namespace NetMetric.CRM.LeadManagement.Application.Commands.Leads;

public sealed class CreateLeadCommandHandler(ILeadAdministrationService administrationService)
    : IRequestHandler<CreateLeadCommand, LeadDetailDto>
{
    public Task<LeadDetailDto> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
        => administrationService.CreateAsync(request, cancellationToken);
}
