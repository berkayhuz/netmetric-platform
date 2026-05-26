// <copyright file="HttpCurrentUserAccessor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NetMetric.Account.Application.Abstractions.Security;

namespace NetMetric.Account.Infrastructure.Security;

public sealed class HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public CurrentUser GetRequired()
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is required for account operations.");

        var user = httpContext.User;
        var tenantId = ReadGuid(user, "tenant_id") ?? ReadGuid(user, "tenantId");
        var userId = ReadGuid(user, ClaimTypes.NameIdentifier) ?? ReadGuid(user, "sub") ?? ReadGuid(user, "user_id");
        var sessionId = ReadGuid(user, "sid") ?? ReadGuid(user, "session_id");
        var authenticatedAt = ReadUnixTime(user, "auth_time")
            ?? ReadUnixTime(user, ClaimTypes.AuthenticationInstant);
        var methods = user.Claims
            .Where(claim => claim.Type == "amr" || claim.Type == ClaimTypes.AuthenticationMethod)
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (tenantId is null || userId is null)
        {
            throw new InvalidOperationException("Authenticated principal must include tenant and user identifiers.");
        }

        return new CurrentUser(
            tenantId.Value,
            userId.Value,
            sessionId,
            authenticatedAt,
            methods,
            httpContext.TraceIdentifier,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            httpContext.Request.Headers["User-Agent"].ToString());
    }

    private static Guid? ReadGuid(ClaimsPrincipal principal, string claimType)
        => Guid.TryParse(principal.Claims.FirstOrDefault(claim => claim.Type == claimType)?.Value, out var value) ? value : null;

    private static DateTimeOffset? ReadUnixTime(ClaimsPrincipal principal, string claimType)
        => long.TryParse(principal.Claims.FirstOrDefault(claim => claim.Type == claimType)?.Value, out var value)
            ? DateTimeOffset.FromUnixTimeSeconds(value)
            : null;
}
