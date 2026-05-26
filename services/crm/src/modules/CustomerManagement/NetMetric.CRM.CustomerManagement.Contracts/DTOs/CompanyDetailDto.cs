// <copyright file="CompanyDetailDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Contracts.DTOs;

public sealed record CompanyDetailDto(
    Guid Id,
    string Name,
    string? TaxNumber,
    string? TaxOffice,
    string? Website,
    string? Email,
    string? Phone,
    string? Sector,
    string? EmployeeCountRange,
    decimal? AnnualRevenue,
    string? Description,
    string? Notes,
    CompanyType CompanyType,
    Guid? OwnerUserId,
    Guid? ParentCompanyId,
    bool IsActive,
    IReadOnlyList<AddressDto> Addresses,
    string RowVersion,
    string? LogoUrl = null);
