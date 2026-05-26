// <copyright file="ICustomerAdministrationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.CustomerManagement.Application.Commands.Customers;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Abstractions;

public interface ICustomerAdministrationService
{
    Task<CustomerDetailDto> CreateAsync(CreateCustomerCommand request, CancellationToken cancellationToken = default);
    Task<CustomerDetailDto> UpdateAsync(UpdateCustomerCommand request, CancellationToken cancellationToken = default);
    Task<CustomerDetailDto> UploadImageAsync(UploadCustomerImageCommand request, CancellationToken cancellationToken = default);
    Task<CustomerDetailDto> RemoveImageAsync(RemoveCustomerImageCommand request, CancellationToken cancellationToken = default);
    Task MarkVipAsync(Guid customerId, bool isVip, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid customerId, CancellationToken cancellationToken = default);
}
