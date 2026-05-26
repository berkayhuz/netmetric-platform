// <copyright file="ICustomerManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Core;

namespace NetMetric.CRM.CustomerManagement.Application.Abstractions.Integration;

public interface ICustomerManagementOutbox
{
    Task EnqueueCustomerCreatedAsync(Customer customer, CancellationToken cancellationToken);

    Task EnqueueCustomerUpdatedAsync(Customer customer, CancellationToken cancellationToken);

    Task EnqueueCustomerDeletedAsync(Customer customer, CancellationToken cancellationToken);
    Task EnqueueCustomerRestoredAsync(Customer customer, CancellationToken cancellationToken);
    Task EnqueueCustomerPurgedAsync(Guid tenantId, Guid customerId, string? customerName, Guid? ownerUserId, CancellationToken cancellationToken);

    Task EnqueueCompanyCreatedAsync(Company company, CancellationToken cancellationToken);

    Task EnqueueCompanyUpdatedAsync(Company company, CancellationToken cancellationToken);

    Task EnqueueCompanyDeletedAsync(Company company, CancellationToken cancellationToken);
    Task EnqueueCompanyRestoredAsync(Company company, CancellationToken cancellationToken);
    Task EnqueueCompanyPurgedAsync(Guid tenantId, Guid companyId, string? companyName, Guid? ownerUserId, CancellationToken cancellationToken);

    Task EnqueueContactCreatedAsync(Contact contact, CancellationToken cancellationToken);

    Task EnqueueContactUpdatedAsync(Contact contact, CancellationToken cancellationToken);

    Task EnqueueContactDeletedAsync(Contact contact, CancellationToken cancellationToken);
    Task EnqueueContactRestoredAsync(Contact contact, CancellationToken cancellationToken);
    Task EnqueueContactPurgedAsync(Guid tenantId, Guid contactId, string? contactName, Guid? ownerUserId, CancellationToken cancellationToken);

    Task EnqueuePrimaryContactChangedAsync(Contact contact, CancellationToken cancellationToken);
}
