// <copyright file="ConvertLeadToCustomerCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;

namespace NetMetric.CRM.LeadManagement.Application.Features.Conversions.Commands.ConvertLeadToCustomer;

public sealed class ConvertLeadToCustomerCommandHandler(ILeadAdministrationService administrationService)
    : IRequestHandler<ConvertLeadToCustomerCommand, LeadConversionResultDto>
{
    public Task<LeadConversionResultDto> Handle(ConvertLeadToCustomerCommand request, CancellationToken cancellationToken)
        => administrationService.ConvertToCustomerAsync(request, cancellationToken);
}
