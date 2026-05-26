// <copyright file="LoginRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Requests;

public sealed record LoginRequest(
    Guid? TenantId,
    string EmailOrUserName,
    string Password,
    bool RememberMe = false,
    string? MfaCode = null,
    string? RecoveryCode = null);
