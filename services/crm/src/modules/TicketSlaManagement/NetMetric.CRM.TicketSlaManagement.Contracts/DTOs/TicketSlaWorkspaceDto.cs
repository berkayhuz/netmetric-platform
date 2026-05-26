// <copyright file="TicketSlaWorkspaceDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketSlaManagement.Contracts.DTOs;

public sealed class TicketSlaWorkspaceDto
{
    public Guid TicketId { get; init; }
    public Guid SlaPolicyId { get; init; }
    public DateTime FirstResponseDueAtUtc { get; init; }
    public DateTime ResolutionDueAtUtc { get; init; }
    public DateTime? FirstRespondedAtUtc { get; init; }
    public DateTime? ResolvedAtUtc { get; init; }
    public bool IsFirstResponseBreached { get; init; }
    public bool IsResolutionBreached { get; init; }
}
