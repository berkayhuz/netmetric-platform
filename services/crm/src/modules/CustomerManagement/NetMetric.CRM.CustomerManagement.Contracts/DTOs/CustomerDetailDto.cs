// <copyright file="CustomerDetailDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Contracts.DTOs;

public sealed record CustomerDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string? Title,
    string? Email,
    string? MobilePhone,
    string? WorkPhone,
    string? PersonalPhone,
    DateTime? BirthDate,
    GenderType Gender,
    string? Department,
    string? JobTitle,
    string? Description,
    string? Notes,
    Guid? OwnerUserId,
    CustomerType CustomerType,
    string? IdentityNumber,
    bool IsVip,
    Guid? CompanyId,
    string? CompanyName,
    bool IsActive,
    IReadOnlyList<AddressDto> Addresses,
    IReadOnlyList<CustomerContactSummaryDto> Contacts,
    string RowVersion,
    string? ImageUrl = null);
