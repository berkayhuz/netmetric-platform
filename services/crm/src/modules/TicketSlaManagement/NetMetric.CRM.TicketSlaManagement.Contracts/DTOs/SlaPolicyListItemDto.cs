// <copyright file="SlaPolicyListItemDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketSlaManagement.Contracts.DTOs;

public sealed class SlaPolicyListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid? TicketCategoryId { get; init; }
    public int Priority { get; init; }
    public int FirstResponseTargetMinutes { get; init; }
    public int ResolutionTargetMinutes { get; init; }
    public bool IsDefault { get; init; }
}
