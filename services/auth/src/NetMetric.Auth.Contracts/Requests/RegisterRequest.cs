// <copyright file="RegisterRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Requests;

public sealed record RegisterRequest(
    string TenantName,
    string UserName,
    string Email,
    string Password,
    string? FirstName,
    string? LastName,
    string? Culture = null);
