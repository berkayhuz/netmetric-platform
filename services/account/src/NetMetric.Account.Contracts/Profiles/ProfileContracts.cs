// <copyright file="ProfileContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Profiles;

public sealed record MyProfileResponse(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string FirstName,
    string LastName,
    string DisplayName,
    string? PhoneNumber,
    string? PhoneCountryIso2,
    string? PhoneCountryCallingCode,
    string? PhoneNationalNumber,
    string? AvatarUrl,
    string? JobTitle,
    string? Department,
    string TimeZone,
    string Culture,
    string Version);

public sealed record UpdateMyProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneCountryIso2,
    string? PhoneNationalNumber,
    string? AvatarUrl,
    string? JobTitle,
    string? Department,
    string TimeZone,
    string Culture,
    string? Version);
