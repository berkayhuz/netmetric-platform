// <copyright file="CustomerWorkspaceDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.DTOs.Customers;

public sealed class CustomerWorkspaceDto
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public string? Email { get; init; }
    public string? MobilePhone { get; init; }
    public string? WorkPhone { get; init; }
    public string? IdentityNumber { get; init; }
    public string? Description { get; init; }
    public Guid? CompanyId { get; init; }
    public string? CompanyName { get; init; }
    public Guid? OwnerUserId { get; init; }
    public bool IsVip { get; init; }
    public bool IsActive { get; init; }
    public int AddressCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
