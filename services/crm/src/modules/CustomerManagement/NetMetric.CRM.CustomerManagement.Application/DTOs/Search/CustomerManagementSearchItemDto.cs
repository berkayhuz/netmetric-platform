// <copyright file="CustomerManagementSearchItemDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.DTOs.Search;

public sealed class CustomerManagementSearchItemDto
{
    public required string EntityType { get; init; }
    public required Guid Id { get; init; }
    public required string DisplayName { get; init; }
    public string? Subtitle { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public Guid? OwnerUserId { get; init; }
    public bool IsActive { get; init; }
    public decimal Score { get; init; }
}
