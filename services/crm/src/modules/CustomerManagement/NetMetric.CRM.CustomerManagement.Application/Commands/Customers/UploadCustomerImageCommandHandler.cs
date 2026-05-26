// <copyright file="UploadCustomerImageCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Customers;

public sealed class UploadCustomerImageCommandHandler(ICustomerAdministrationService administrationService)
    : IRequestHandler<UploadCustomerImageCommand, CustomerDetailDto>
{
    public Task<CustomerDetailDto> Handle(UploadCustomerImageCommand request, CancellationToken cancellationToken)
        => administrationService.UploadImageAsync(request, cancellationToken);
}
