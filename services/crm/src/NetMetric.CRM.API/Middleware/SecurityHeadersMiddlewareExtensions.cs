// <copyright file="SecurityHeadersMiddlewareExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.API.Middleware;

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseCrmSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers.TryAdd("X-Content-Type-Options", "nosniff");
                headers.TryAdd("X-Frame-Options", "DENY");
                headers.TryAdd("Referrer-Policy", "no-referrer");
                headers.TryAdd("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'");
                headers.TryAdd("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
                headers.TryAdd("X-Permitted-Cross-Domain-Policies", "none");
                headers.TryAdd("Cross-Origin-Opener-Policy", "same-origin");
                headers.TryAdd("Cross-Origin-Resource-Policy", "same-origin");
                headers.TryAdd("Cache-Control", "no-store");
                headers.TryAdd("Pragma", "no-cache");

                return Task.CompletedTask;
            });

            await next();
        });
}
