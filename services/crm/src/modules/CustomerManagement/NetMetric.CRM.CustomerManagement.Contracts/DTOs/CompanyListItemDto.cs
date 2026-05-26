// <copyright file="CompanyListItemDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Contracts.DTOs;

public sealed record CompanyListItemDto(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    CompanyType CompanyType,
    string? Sector,
    bool IsActive,
    int ContactCount,
    string RowVersion,
    string? LogoUrl = null);
