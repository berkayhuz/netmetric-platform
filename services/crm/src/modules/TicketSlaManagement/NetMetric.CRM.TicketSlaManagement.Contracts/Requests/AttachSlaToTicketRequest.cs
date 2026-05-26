// <copyright file="AttachSlaToTicketRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketSlaManagement.Contracts.Requests;

public sealed class AttachSlaToTicketRequest
{
    public Guid TicketId { get; init; }
    public Guid SlaPolicyId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
