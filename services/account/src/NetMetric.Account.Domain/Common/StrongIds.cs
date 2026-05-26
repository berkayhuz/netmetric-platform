// <copyright file="StrongIds.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Domain.Common;

public readonly record struct TenantId(Guid Value)
{
    public static TenantId From(Guid value) => value == Guid.Empty
        ? throw new DomainValidationException("Tenant id cannot be empty.")
        : new TenantId(value);
}

public readonly record struct UserId(Guid Value)
{
    public static UserId From(Guid value) => value == Guid.Empty
        ? throw new DomainValidationException("User id cannot be empty.")
        : new UserId(value);
}

public readonly record struct UserSessionId(Guid Value)
{
    public static UserSessionId From(Guid value) => value == Guid.Empty
        ? throw new DomainValidationException("User session id cannot be empty.")
        : new UserSessionId(value);
}
