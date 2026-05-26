// <copyright file="SecurityHeadersMiddleware.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;
using NetMetric.AspNetCore.Security;
using NetMetric.Auth.API.Security;

namespace NetMetric.Auth.API.Middlewares;

public sealed class SecurityHeadersMiddleware(RequestDelegate next, IOptions<ApiSecurityHeadersOptions> options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var value = options.Value;
        SecuritySupport.ApplySecurityHeaders(
            context,
            new SecurityHeadersValues(
                value.ContentSecurityPolicy,
                value.ReferrerPolicy,
                value.PermissionsPolicy,
                value.EnableHsts,
                value.HstsMaxAgeSeconds,
                value.PreloadHsts,
                value.IncludeSubDomains,
                DisableResponseCaching: true));

        await next(context);
    }
}
