// <copyright file="CompanyWorkspaceDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.DTOs.Companies;

public sealed class CompanyWorkspaceDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Website { get; init; }
    public string? TaxNumber { get; init; }
    public string? TaxOffice { get; init; }
    public string? Sector { get; init; }
    public string? Description { get; init; }
    public Guid? OwnerUserId { get; init; }
    public Guid? ParentCompanyId { get; init; }
    public bool IsActive { get; init; }
    public int ContactCount { get; init; }
    public int CustomerCount { get; init; }
    public int AddressCount { get; init; }
    public int ChildCompanyCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
