// <copyright file="AddCompanyAddressCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Addresses;

public sealed class AddCompanyAddressCommandHandler(IAddressAdministrationService administrationService)
    : IRequestHandler<AddCompanyAddressCommand, AddressDto>
{
    public Task<AddressDto> Handle(AddCompanyAddressCommand request, CancellationToken cancellationToken)
        => administrationService.AddToCompanyAsync(request, cancellationToken);
}
