// <copyright file="IOpportunityManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Sales;

namespace NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;

public interface IOpportunityManagementOutbox
{
    Task EnqueueOpportunityCreatedAsync(Opportunity opportunity, CancellationToken cancellationToken);

    Task EnqueueOpportunityCreatedAndPersistAsync(Opportunity opportunity, CancellationToken cancellationToken);

    Task EnqueueOpportunityUpdatedAsync(Opportunity opportunity, CancellationToken cancellationToken);

    Task EnqueueOpportunityDeletedAsync(Opportunity opportunity, CancellationToken cancellationToken);

    Task EnqueueOpportunityRestoredAsync(Opportunity opportunity, CancellationToken cancellationToken);

    Task EnqueueOpportunityPurgedAsync(Guid tenantId, Guid opportunityId, string? opportunityName, Guid? ownerUserId, CancellationToken cancellationToken);
}
