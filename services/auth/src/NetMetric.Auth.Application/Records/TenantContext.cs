// <copyright file="TenantContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Records;

public sealed record TenantContext(Guid? TenantId, string? TenantSlug, string Source, bool IsTrusted);
