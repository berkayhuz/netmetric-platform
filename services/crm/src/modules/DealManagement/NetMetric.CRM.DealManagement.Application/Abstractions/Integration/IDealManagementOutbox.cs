// <copyright file="IDealManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Sales;

namespace NetMetric.CRM.DealManagement.Application.Abstractions.Integration;

public interface IDealManagementOutbox
{
    Task EnqueueDealCreatedAsync(Deal deal, CancellationToken cancellationToken);
    Task EnqueueDealCreatedAndPersistAsync(Deal deal, CancellationToken cancellationToken);

    Task EnqueueDealUpdatedAsync(Deal deal, CancellationToken cancellationToken);

    Task EnqueueDealDeletedAsync(Deal deal, CancellationToken cancellationToken);

    Task EnqueueDealRestoredAsync(Deal deal, CancellationToken cancellationToken);

    Task EnqueueDealPurgedAsync(Guid tenantId, Guid dealId, string? dealName, Guid? ownerUserId, CancellationToken cancellationToken);
}

