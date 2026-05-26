// <copyright file="IGlobalTrashIndexWriter.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Core;

namespace NetMetric.CRM.CustomerManagement.Application.Abstractions;

public interface IGlobalTrashIndexWriter
{
    Task AddContactDeletionAsync(Contact contact, CancellationToken cancellationToken = default);
    Task AddCustomerDeletionAsync(Customer customer, CancellationToken cancellationToken = default);
    Task AddCompanyDeletionAsync(Company company, CancellationToken cancellationToken = default);
}
