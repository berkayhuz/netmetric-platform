// <copyright file="RequireTenantContextMiddleware.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Claims;

namespace NetMetric.CRM.API.Middleware;

public sealed class RequireTenantContextMiddleware(
    RequestDelegate next,
    ILogger<RequireTenantContextMiddleware> logger)
{
    private static readonly string[] TenantClaimTypes = ["tenant_id", "tenantId", "tenant"];

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true && !HasTenantClaim(context.User))
        {
            logger.LogWarning("Authenticated CRM request rejected because no tenant claim was present.");

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await Results.Problem(
                title: "Tenant context required",
                detail: "Authenticated CRM requests must include a valid tenant claim.",
                statusCode: StatusCodes.Status403Forbidden)
                .ExecuteAsync(context);
            return;
        }

        await next(context);
    }

    private static bool HasTenantClaim(ClaimsPrincipal user) =>
        TenantClaimTypes
            .Select(type => user.FindFirst(type)?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Any(value => Guid.TryParse(value, out var tenantId) && tenantId != Guid.Empty);
}
