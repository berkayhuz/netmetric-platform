// <copyright file="ConfirmEmailRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Requests;

public sealed record ConfirmEmailRequest(Guid TenantId, Guid UserId, string Token);
