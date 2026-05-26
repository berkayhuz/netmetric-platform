// <copyright file="WorkspaceSummaryResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Responses;

public sealed record WorkspaceSummaryResponse(
    Guid TenantId,
    string Name,
    string? Slug,
    string? Role,
    bool IsDefault,
    DateTimeOffset? LastUsedAt);
