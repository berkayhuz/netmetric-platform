// <copyright file="AuthSessionResult.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.Contracts.Responses;

namespace NetMetric.Auth.Application.Records;

public abstract record AuthSessionResult
{
    private AuthSessionResult()
    {
    }

    public sealed record Issued(AuthenticationTokenResponse Tokens) : AuthSessionResult;

    public sealed record PendingConfirmation(Guid TenantId, Guid UserId, string Email) : AuthSessionResult;
}
