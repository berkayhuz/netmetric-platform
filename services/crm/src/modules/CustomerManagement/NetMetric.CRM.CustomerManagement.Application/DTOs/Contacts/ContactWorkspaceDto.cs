// <copyright file="ContactWorkspaceDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.DTOs.Contacts;

public sealed class ContactWorkspaceDto
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }
    public string? Title { get; init; }
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
    public string? Email { get; init; }
    public string? MobilePhone { get; init; }
    public string? WorkPhone { get; init; }
    public string? PersonalPhone { get; init; }
    public string? Description { get; init; }
    public Guid? CompanyId { get; init; }
    public string? CompanyName { get; init; }
    public Guid? CustomerId { get; init; }
    public string? CustomerName { get; init; }
    public Guid? OwnerUserId { get; init; }
    public bool IsPrimaryContact { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
