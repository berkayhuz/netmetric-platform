// <copyright file="CustomerListItemDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Contracts.DTOs;

public sealed record CustomerListItemDto(
    Guid Id,
    string FullName,
    string? Email,
    string? MobilePhone,
    CustomerType CustomerType,
    bool IsVip,
    string? CompanyName,
    bool IsActive,
    DateTime CreatedAt,
    string RowVersion,
    string? ImageUrl = null);
