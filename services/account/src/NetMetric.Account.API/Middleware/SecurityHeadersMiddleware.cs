// <copyright file="SecurityHeadersMiddleware.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;
using NetMetric.Account.Api.Security;

namespace NetMetric.Account.Api.Middleware;

public sealed class SecurityHeadersMiddleware(RequestDelegate next, IOptions<AccountSecurityHeadersOptions> options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers.TryAdd("Content-Security-Policy", options.Value.ContentSecurityPolicy);
        headers.TryAdd("X-Frame-Options", "DENY");
        headers.TryAdd("X-Content-Type-Options", "nosniff");
        headers.TryAdd("Referrer-Policy", options.Value.ReferrerPolicy);
        headers.TryAdd("Permissions-Policy", options.Value.PermissionsPolicy);

        await next(context);
    }
}
