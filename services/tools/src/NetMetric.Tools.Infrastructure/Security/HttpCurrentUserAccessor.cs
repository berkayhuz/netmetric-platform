// <copyright file="HttpCurrentUserAccessor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NetMetric.Tools.Application.Abstractions.Security;

namespace NetMetric.Tools.Infrastructure.Security;

public sealed class HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public CurrentUser GetRequired()
    {
        var context = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HTTP context is required.");

        var principal = context.User;
        var userId = ReadGuid(principal, ClaimTypes.NameIdentifier) ?? ReadGuid(principal, "sub") ?? ReadGuid(principal, "user_id");

        if (userId is null || userId == Guid.Empty)
        {
            throw new InvalidOperationException("Authenticated principal must include a valid user id.");
        }

        return new CurrentUser(
            userId.Value,
            context.TraceIdentifier,
            context.Connection.RemoteIpAddress?.ToString(),
            context.Request.Headers.UserAgent.ToString());
    }

    private static Guid? ReadGuid(ClaimsPrincipal principal, string claimType)
        => Guid.TryParse(principal.FindFirstValue(claimType), out var value) ? value : null;
}
