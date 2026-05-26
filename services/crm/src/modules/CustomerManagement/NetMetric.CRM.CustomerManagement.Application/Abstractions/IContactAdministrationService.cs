// <copyright file="IContactAdministrationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.CustomerManagement.Application.Commands.Contacts;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Abstractions;

public interface IContactAdministrationService
{
    Task<ContactDetailDto> CreateAsync(CreateContactCommand request, CancellationToken cancellationToken = default);
    Task<ContactDetailDto> UpdateAsync(UpdateContactCommand request, CancellationToken cancellationToken = default);
    Task SetPrimaryAsync(Guid contactId, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid contactId, CancellationToken cancellationToken = default);
    Task RestoreFromTrashAsync(Guid trashItemId, CancellationToken cancellationToken = default);
    Task<int> PurgeExpiredTrashItemsAsync(int batchSize = 100, CancellationToken cancellationToken = default);
    Task<int> PurgeExpiredTrashItemsForTenantAsync(Guid tenantId, int batchSize = 100, CancellationToken cancellationToken = default);
}
