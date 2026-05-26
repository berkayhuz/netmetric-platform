// <copyright file="AddressDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Contracts.DTOs;

public sealed record AddressDto(
    Guid Id,
    AddressType AddressType,
    string Line1,
    string? Line2,
    string? District,
    string? City,
    string? State,
    string? Country,
    string? ZipCode,
    bool IsDefault,
    string RowVersion);
