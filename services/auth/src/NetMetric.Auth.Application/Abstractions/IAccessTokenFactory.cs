// <copyright file="IAccessTokenFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.Application.Descriptors;
using NetMetric.Auth.Domain.Entities;

namespace NetMetric.Auth.Application.Abstractions;

public interface IAccessTokenFactory
{
    AccessTokenDescriptor Create(User user, Guid tenantId, Guid sessionId);
    AccessTokenDescriptor Create(
        Guid userId,
        string userName,
        string email,
        int tokenVersion,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions,
        Guid tenantId,
        Guid sessionId,
        DateTimeOffset? authenticatedAt = null,
        IReadOnlyCollection<string>? authenticationMethods = null);
}
