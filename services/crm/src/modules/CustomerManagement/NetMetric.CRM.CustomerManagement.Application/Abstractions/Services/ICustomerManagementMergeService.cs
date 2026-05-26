// <copyright file="ICustomerManagementMergeService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.Abstractions.Services;

public interface ICustomerManagementMergeService
{
    Task MergeCompaniesAsync(Guid targetCompanyId, Guid sourceCompanyId, CancellationToken cancellationToken = default);
    Task MergeContactsAsync(Guid targetContactId, Guid sourceContactId, CancellationToken cancellationToken = default);
    Task MergeCustomersAsync(Guid targetCustomerId, Guid sourceCustomerId, CancellationToken cancellationToken = default);
}
