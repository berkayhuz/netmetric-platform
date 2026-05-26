// <copyright file="AddCustomerAddressCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Addresses;

public sealed class AddCustomerAddressCommandHandler(IAddressAdministrationService administrationService)
    : IRequestHandler<AddCustomerAddressCommand, AddressDto>
{
    public Task<AddressDto> Handle(AddCustomerAddressCommand request, CancellationToken cancellationToken)
        => administrationService.AddToCustomerAsync(request, cancellationToken);
}
