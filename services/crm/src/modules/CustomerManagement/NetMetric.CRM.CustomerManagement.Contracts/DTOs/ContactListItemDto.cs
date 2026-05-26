// <copyright file="ContactListItemDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Contracts.DTOs;

public sealed record ContactListItemDto(
    Guid Id,
    string FullName,
    string? Email,
    string? MobilePhone,
    string? CompanyName,
    string? CustomerName,
    bool IsPrimaryContact,
    bool IsActive,
    string RowVersion);
