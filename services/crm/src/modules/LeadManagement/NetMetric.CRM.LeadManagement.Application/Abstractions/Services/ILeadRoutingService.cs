// <copyright file="ILeadRoutingService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.LeadManagement.Application.Abstractions.Services;

public interface ILeadRoutingService
{
    Task RouteLeadAsync(Guid leadId, CancellationToken cancellationToken);
}
