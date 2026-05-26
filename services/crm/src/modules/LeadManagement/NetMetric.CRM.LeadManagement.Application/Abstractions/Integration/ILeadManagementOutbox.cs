// <copyright file="ILeadManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Sales;

namespace NetMetric.CRM.LeadManagement.Application.Abstractions.Integration;

public interface ILeadManagementOutbox
{
    Task EnqueueLeadCreatedAsync(Lead lead, CancellationToken cancellationToken);

    Task EnqueueLeadUpdatedAsync(Lead lead, CancellationToken cancellationToken);

    Task EnqueueLeadDeletedAsync(Lead lead, CancellationToken cancellationToken);

    Task EnqueueLeadRestoredAsync(Lead lead, CancellationToken cancellationToken);

    Task EnqueueLeadPurgedAsync(Guid tenantId, Guid leadId, string? leadName, Guid? ownerUserId, CancellationToken cancellationToken);
}
