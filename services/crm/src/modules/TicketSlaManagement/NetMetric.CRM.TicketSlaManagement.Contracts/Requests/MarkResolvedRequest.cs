// <copyright file="MarkResolvedRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketSlaManagement.Contracts.Requests;

public sealed class MarkResolvedRequest
{
    public Guid TicketId { get; init; }
    public DateTime ResolvedAtUtc { get; init; }
}
